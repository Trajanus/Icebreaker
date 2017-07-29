using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using static Icebreaker.Enums;

// third party
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Icebreaker
{
    class Program
    {
        // search terms
        private static string[] termsList = new string[9] { "firefly", "game of thrones", "asoif", "nerd", "honest", "zelda", "board game", "boardgame", "history" };
        private static string termsRegex = String.Format("({0})", String.Join("|", termsList));

        // These are used to rate limit http requests
        private static Stopwatch downloadTimer = new Stopwatch();
        private static TimeSpan downloadDelay = new TimeSpan(0, 0, 1);
        private const int MinimumDownloadDelaySeconds = 15;
        private const int MaximumDownloadDelaySeconds = 25;

        // This is a query string parameter required 
        // by okcupid's send message url.
        private static string authcode;

        // Profiles that the program has processed either in a
        // previous run or during the current session.
        private static HashSet<string> ProcessedProfiles;

        private static string JsonStartPattern = ConfigurationManager.AppSettings[Constants.AppConfigKeys.MatchSummaryJsonStartPattern];
        private static string JsonEndPattern = ConfigurationManager.AppSettings[Constants.AppConfigKeys.MatchSummaryJsonEndPattern];

        private static string OkcUserId = String.Empty;

        

        public static bool CleanupFiles = Convert.ToBoolean(ConfigurationManager.AppSettings[Constants.AppConfigKeys.CleanupFiles]);

        private static DocumentRetriever Authenticate(UserInput input)
        {
            string userAgent = "User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36"; //ConfigurationManager.AppSettings[Constants.AppConfigKeys.UserAgent];
            bool SendMessages = Convert.ToBoolean(ConfigurationManager.AppSettings[Constants.AppConfigKeys.SendMessages]);

            DocumentRetriever okcRetriever = new DocumentRetriever(Constants.OkcUri.OkcUrl.ToString(), userAgent, SendMessages);

            CookieContainer authenticationCookies = Storage.GetAuthenticationCookies();
            
            if (0 == authenticationCookies.Count)
            {
                string username = input.UsernameCollect();
                string password = input.PasswordCollect();
                okcRetriever.AuthCookies = okcRetriever.GetAuthCookieList(username, password);
                Storage.SaveAuthenticationCookies(okcRetriever.AuthCookies);
            }
            else
            {
                okcRetriever.AuthCookies = authenticationCookies;
            }

            return okcRetriever;
        }

        private static DocumentRetriever Initialize(UserInput input)
        {
            ProcessedProfiles = Storage.Initialize();
            DocumentRetriever okcRetriever = Authenticate(input);

            // Go to the user's okcupid profile page and parse
            // it to find the authcode in the html.
            HtmlDocument personalProfilePage = DownloadOkcPage(okcRetriever, Constants.OkcUri.ProfileHomepage.ToString());
            Match authcodePattern = Regex.Match(personalProfilePage.DocumentNode.InnerText, Constants.AuthcodeRegex.ToString());
            authcode = authcodePattern.Value.Split('"')[1];

            OkcUserId = ExtractUserIdFromProfilePage(personalProfilePage.DocumentNode.InnerText);

            return okcRetriever;
        }

        static void Main(string[] args)
        {
            try
            {
                UserInput userInputCollector = new UserInput();
                DocumentRetriever okcRetriever = Initialize(userInputCollector);

                
                MessageRunType runType = userInputCollector.MessageRunTypeCollect();

                switch (runType)
                {
                    case MessageRunType.SingleMessageRun:
                        SingleMessageRun messageRun = SetupSingleMessageRun(userInputCollector);
                        SendMessageToUsernames(messageRun, okcRetriever);
                        break;
                    case MessageRunType.SingleMessageSearch:
                        SingleMessageSearch searchRun = userInputCollector.SingleMessageSearchRunCollect();
                        SendMessageToMatches(searchRun,
                            okcRetriever);
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("The program has crashed: {0}", ex.Message);
                Console.WriteLine("Stacktrace: \n{0}", ex.StackTrace);
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
            finally
            {
                Storage.MarkProcessedProfiles(ProcessedProfiles.ToList<string>());
                if(CleanupFiles)
                    Storage.Cleanup();
            }
        }

        public static void SendMessageToMatches(SingleMessageSearch searchRun,
            DocumentRetriever okcRetriever)
        {
            int messagesSent = 0;
            int profilesProcessedThisRun = 0;

            while (messagesSent < searchRun.NumMessagesToSend)
            {

                List<MatchSummary.Datum> matchSummaries = GetMatchSummeries(okcRetriever);
                matchSummaries = matchSummaries.Where(ms => ms.percentages.match >= searchRun.MinMatchPercentage
                    && ms.percentages.match <= searchRun.MaxMatchPercentage
                    && ms.userinfo.age >= searchRun.MinAge
                    && ms.userinfo.age <= searchRun.MaxAge
                    && searchRun.GendersSearchingFor.Contains((Gender)(Enum.Parse(typeof(Gender),ms.userinfo.gender)))
                    && searchRun.OrientationsSearchingFor.Contains((Orientation)(Enum.Parse(typeof(Orientation), ms.userinfo.orientation))) 
                    && !ms.online).ToList();

                foreach (MatchSummary.Datum matchSummary in matchSummaries)
                {
                    if (messagesSent >= searchRun.NumMessagesToSend)
                        break;

                    HtmlDocument profilePageHtml = OkcPageGet(okcRetriever
                        , matchSummary.username
                        , ProfilePage.ProfilePageGetUri(matchSummary.username)
                        , Storage.ProfilePageGetFilepath(matchSummary.username));

                    ProfilePage currProfilePage = new ProfilePage(profilePageHtml, matchSummary);

                    if (currProfilePage.IsSearchable)
                    {
                        bool hasBodyTypeSearchingFor = currProfilePage.HasBodyTypeSearchingFor(searchRun.BodyTypesSearchingFor);
                        bool hasEthnicitySearchingFor = currProfilePage.HasEthnicitySearchingFor(searchRun.EthnicitiesSearchingFor);
                        bool hasRelationshipStatusSearchingFor = currProfilePage.HasRelationshipStatus(searchRun.RelationshipStatuses);
                        bool hasAllowedKidsStatus = searchRun.KidsAllowed;

                        if (!searchRun.KidsAllowed)
                            hasAllowedKidsStatus = !currProfilePage.HasKids();

                        if (hasBodyTypeSearchingFor
                            && hasEthnicitySearchingFor
                            && hasAllowedKidsStatus
                            && hasRelationshipStatusSearchingFor
                            && !matchSummary.online)
                        {
                            string postData = String.Format("{{\"receiverid\":{0},\"body\":\"{1}\",\"source\":\"desktop_global\",\"service\":\"profile\",\"reply\":\"false\"}}"
                                                            , matchSummary.userid
                                                            , searchRun.MessageText);


                            okcRetriever.SendOkCupidMessage(Constants.OkcUri.SendMessageUrl.ToString()
                                                            , Constants.Http.AcceptHeaderDefault
                                                            , Constants.Http.ContentTypeJson
                                                            , postData
                                                            , authcode);
                            messagesSent++;
                        }
                    }
                    ProcessedProfiles.Add(matchSummary.username);

                    profilesProcessedThisRun++;
                    Console.WriteLine("Processed profile: " + matchSummary.username);
                    Console.WriteLine("Total profiles processed: " + profilesProcessedThisRun);
                }
            }

        }

        public static SingleMessageRun SetupSingleMessageRun(UserInput input)
        {
            string messageText = input.DefaultMessageCollect();

            Message message = new Message(messageText);
            SingleMessageRun messageRun = new SingleMessageRun(Storage.UsernamesToMessageGet(), message);

            if (0 == messageRun.Usernames.Count)
            {
                Console.WriteLine(string.Format("No user profiles entered to send messages to, please enter profile names (one per line) into this file\n\n{0}", Storage.UsernamesToMessageFilePath));
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            return messageRun;
        }

        public static void SendMessageToUsernames(SingleMessageRun messageRun,
            DocumentRetriever okcRetriever)
        {
            string usernamesToMessage = "";

            usernamesToMessage = string.Join(", ", messageRun.Usernames);
            usernamesToMessage.TrimEnd(',');

            Console.WriteLine(string.Format("\n\nMessaging the following user profiles: \n{0} \n\nWith the message of: \n\"{1}\" \n\nProceed? (y/n)", usernamesToMessage, messageRun.MessageText));

            if (Console.ReadLine() == "n")
                Environment.Exit(0);

            foreach (string username in messageRun.Usernames)
            {
                HtmlDocument profilePageHtml = OkcPageGet(okcRetriever
                    , username
                    , ProfilePage.ProfilePageGetUri(username)
                    , Storage.ProfilePageGetFilepath(username));

                string userid = ExtractUserIdFromProfilePage(profilePageHtml.DocumentNode.InnerHtml);

                string postData = String.Format("{{\"receiverid\":{0},\"body\":\"{1}\",\"source\":\"desktop_global\",\"service\":\"profile\",\"profile_tab\":\"profile\",\"reply\":\"0\"}}", userid, messageRun.MessageText);


                okcRetriever.SendOkCupidMessage(Constants.OkcUri.SendMessageUrl.ToString()
                                                , Constants.Http.AcceptHeaderDefault
                                                , Constants.Http.ContentTypeJson
                                                , postData
                                                , authcode);

                ProcessedProfiles.Add(username);
            }
        }

        public static List<MatchSummary.Datum> GetMatchSummeries(DocumentRetriever okcRetriever)
        {
            string matchSummaryPageHtml = DownloadOkcPage(okcRetriever, Constants.OkcUri.MatchPageUrlPart).DocumentNode.OuterHtml;

            MatchSummary root = ExtractMatchSummaries(matchSummaryPageHtml
                                                        , JsonStartPattern
                                                        , JsonEndPattern);

            List<MatchSummary.Datum> matchSummaries = root.data.OrderByDescending(item => item.percentages.match).ToList<MatchSummary.Datum>();
            matchSummaries.RemoveAll(match => ProcessedProfiles.Contains(match.username));

            return matchSummaries;
        }

        public static void ProcessMatches(DocumentRetriever okcRetriever)
        {
            List<MatchSummary.Datum> matchSummaries = GetMatchSummeries(okcRetriever);

            List<QuestionBasedMessage> availableMessages = Storage.GetQuestionBasedMessages();

            if (0 == availableMessages.Count)
            {
                Console.WriteLine(string.Format("No messages defined.  Add messages to the following file and then rerun the program: {0}.\n Press any key to exit.", Storage.MessagesFilePath));
                Environment.Exit(0);
            }

            ProfilePage currProfilePage;
            HtmlDocument profilePageHtml;

            HtmlDocument questionsPage;

            bool sentMessage = false;
            foreach (MatchSummary.Datum matchSummary in matchSummaries)
            {
                string matchedTerms = "";

                profilePageHtml = OkcPageGet(okcRetriever
                    , matchSummary.username
                    , ProfilePage.ProfilePageGetUri(matchSummary.username)
                    , Storage.ProfilePageGetFilepath(matchSummary.username));

                currProfilePage = new ProfilePage(profilePageHtml, matchSummary);

                if (currProfilePage.PhotoCount() <= 1)
                {
                    Console.WriteLine("Profile " + matchSummary.username + " is excluded based on insufficient photographs.");
                    ProcessedProfiles.Add(matchSummary.username);
                    continue;
                }

                //if (currProfilePage.HasExcludedBodyType(excludedBodyTypeRegex))
                //{
                //    string matchExcludedBodyTypeResultText = String.Format("{0} excluded based on body type - Profile Link: {1}\n"
                //                                    , currProfilePage.Username
                //                                    , currProfilePage.ProfileUri.ToString());

                //    Storage.StoreMatchResult(matchExcludedBodyTypeResultText, Storage.MatchStatus.ExcludedBodyType);
                //    Console.WriteLine(matchExcludedBodyTypeResultText);

                //    ProcessedProfiles.Add(matchSummary.username);
                //    continue;
                //}

                if (currProfilePage.MatchesSearchTerm(termsList, termsRegex, out matchedTerms))
                {
                    string matchResultText = String.Format("{0} matched on term(s): {1} - Profile Link: {2}\n"
                                                            , currProfilePage.Username
                                                            , matchedTerms
                                                            , currProfilePage.ProfileUri.ToString());

                    Storage.StoreMatchResult(matchResultText, Storage.MatchStatus.MatchesSearchTerm);

                    Console.WriteLine(matchResultText);
                }
                else
                {
                    string nonMatchResultText = "Profile " + currProfilePage.Username + " is NOT a match.\n";

                    Storage.StoreMatchResult(nonMatchResultText, Storage.MatchStatus.NoMatchingSearchTerm);
                    Console.WriteLine(nonMatchResultText);
                }

                int questionNumberStep = 1;
                do
                {
                    questionsPage = OkcPageGet(okcRetriever
                        , matchSummary.username
                        , QuestionsPageGetUri(matchSummary.username, questionNumberStep)
                        , Storage.QuestionsPageGetFilepath(matchSummary.username, questionNumberStep));

                    HtmlNode answerNode;
                    foreach (QuestionBasedMessage message in availableMessages)
                    {
                        answerNode = questionsPage.DocumentNode.SelectSingleNode(message.AnswerHtmlId_XpathQuery);
                        if (answerNode != null)
                        {
                            Console.WriteLine("Found a match for the question: " + message.Question + " with profile: " + matchSummary.username + "\n");
                            if (answerNode.InnerText.Trim().ToLower() == message.Answer.Trim().ToLower())
                            {
                                Console.WriteLine("Found matching answer: " + message.Answer);

                                string postData = String.Format("{{\"receiverid\":{0},\"body\":\"{1}\",\"source\":\"desktop_global\",\"service\":\"profile\",\"profile_tab\":\"profile\",\"reply\":\"0\"}}", matchSummary.userid, message.MessageText);

                                okcRetriever.SendOkCupidMessage(Constants.OkcUri.SendMessageUrl.ToString()
                                                                , Constants.Http.AcceptHeaderDefault
                                                                , Constants.Http.ContentTypeJson
                                                                , postData
                                                                , authcode);

                                ProcessedProfiles.Add(matchSummary.username);
                                sentMessage = true;
                            }
                        }

                    }

                    questionNumberStep += Constants.QuestionsPerPageStepValue;
                }
                while (null == questionsPage.DocumentNode.SelectNodes(Constants.XPathSelector.NextQuestionsLinkDisabled)
                        && (!sentMessage));
            }

            Console.ReadLine();

        }

        /// <summary>
        /// Downloads the page at urlPath as userAgent.  Note that this will wait until
        /// downloadDelay has elapsed before making the request and will reset downloadDelay
        /// to a random integer between MinimumDownloadDelaySeconds and MaximumDownloadDelaySeconds.
        /// </summary>
        /// <param name="urlPath"></param>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public static HtmlDocument DownloadOkcPage(DocumentRetriever okcRetriever,
            string urlPath)
        {
            HtmlDocument okCupidPage;
            Uri okCupidUri = new Uri(Constants.OkcUri.OkcUrl, urlPath);

            if (downloadTimer.IsRunning &&
                downloadTimer.Elapsed < downloadDelay)
            {
                Thread.Sleep((int)(downloadDelay.TotalMilliseconds - downloadTimer.ElapsedMilliseconds));

                Random r = new Random();
                int nextDownloadSecondsDelay = r.Next(MinimumDownloadDelaySeconds, MaximumDownloadDelaySeconds);
                downloadDelay = new TimeSpan(0, 0, nextDownloadSecondsDelay);
            }

            okCupidPage = okcRetriever.RetrieveHtmlDocument(okCupidUri.ToString(), Constants.Http.AcceptHeaderDefault);
            downloadTimer.Restart();
            
            return okCupidPage;
        }

        /// <summary>
        /// Returns an HtmlDocument from a cached text file if one exists for the given username
        /// else it constructs a profile page link for the given username and downloads the
        /// page from okcupid.com and saves it to the given filepath.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="url"></param>
        /// <param name="userAgent"></param>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private static HtmlDocument OkcPageGet(DocumentRetriever okcRetriever, string username, Uri url, string filepath)
        {
            HtmlDocument page;

            if (File.Exists(filepath))
            {
                page = new HtmlDocument();
                page.LoadHtml(File.ReadAllText(filepath));
            }
            else
            {
                page = DownloadOkcPage(okcRetriever, url.ToString());
                File.WriteAllText(filepath, page.DocumentNode.OuterHtml);
            }

            return page;
        }

        /// <summary>
        /// Constructs a Uri to the given user's answered questions
        /// for the page associated with the given questionNumberStep.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="questionNumberStep"></param>
        /// <returns></returns>
        private static Uri QuestionsPageGetUri(string username, int questionNumberStep)
        {
            string urlPath = Path.Combine(Constants.OkcUri.ProfilePageUrlPart + username + Constants.OkcUri.QuestionsUrlPart + questionNumberStep.ToString());
            return new Uri(Constants.OkcUri.OkcUrl, urlPath);
        }

        /// <summary>
        /// This method extracts MatchSummary objects from the JSON on a match summary
        /// HTML page.  jsonStartPattern is the regex pattern that marks the start of JSON section
        // of the page,  jsonEndPattern is the ending regex pattern.
        /// </summary>
        /// <param name="matchPageHtml"></param>
        /// <param name="jsonStartPattern"></param>
        /// <param name="jsonEndPattern"></param>
        /// <returns></returns>
        public static MatchSummary ExtractMatchSummaries(string matchPageHtml, string jsonStartPattern, string jsonEndPattern)
        {
            MatchSummary matchSummaries;
            string matchesJsonString;

            string startPoint = Regex.Escape(jsonStartPattern);
            string endPoint = Regex.Escape(jsonEndPattern);

            Match startPointMatch = Regex.Match(matchPageHtml, startPoint);
            Match endPointMatch = Regex.Match(matchPageHtml, endPoint);

            int jsonStringLength = endPointMatch.Index - startPointMatch.Index;

            if (jsonStringLength <= 0)
                throw new Exception("JSON string was not found in the input");

            matchesJsonString = "{" + matchPageHtml.Substring(startPointMatch.Index, jsonStringLength) + "}]}";

            matchSummaries = JsonConvert.DeserializeObject<MatchSummary>(matchesJsonString);
            return matchSummaries;
        }

        /// <summary>
        /// Retrieves the userid associated with the given
        /// profile page html.
        /// </summary>
        /// <param name="profilePageHtml"></param>
        /// <returns></returns>
        public static string ExtractUserIdFromProfilePage(string profilePageHtml)
        {
            string matchString = "\"userid\" : \"([0-9]+)\"";;
            if (!String.IsNullOrEmpty(OkcUserId))
            {
                string ownUserIdMatchString = "\"userid\" : \"(" + OkcUserId.ToString() + ")\"";
                profilePageHtml = Regex.Replace(profilePageHtml, ownUserIdMatchString, "");
            }

            Match match = Regex.Match(profilePageHtml, matchString);

            return match.Groups[1].Value;
        }

    }
}
