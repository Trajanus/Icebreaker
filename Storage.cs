using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using static Icebreaker.Enums;

namespace Icebreaker
{
    public static class Storage
    {
        // file related
        private static readonly string ProgramDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            , "Icebreaker");
        private static readonly string matchesDirectory = Path.Combine(ProgramDataDirectory, "matches");
        private static readonly string bodyTypeExclusionsDirectory = Path.Combine(ProgramDataDirectory, "bodytypeexclusions");
        private static readonly string nonMatchesDirectory = Path.Combine(ProgramDataDirectory, "nonmatches");
        private static readonly string userPreferencesDirectory = Path.Combine(ProgramDataDirectory, "userpreferences");

        private static readonly string ProfilePageDirectory = Path.Combine(ProgramDataDirectory, "profiles");
        private static readonly string QuestionsPageDirectory = Path.Combine(ProgramDataDirectory, "questions");

        private static readonly string MatchHtmlPath = Path.Combine(ProgramDataDirectory, "matchpage.txt");
        private static readonly string MatchJsonPath = Path.Combine(ProgramDataDirectory, "matchJSON.txt");
        

        private static readonly string processedProfilesPath = Path.Combine(ProgramDataDirectory, "processedprofiles.txt");

        private static readonly string SearchTermsPath = Path.Combine(ProgramDataDirectory, "searchterms.txt");

        // user preferences files
        private static readonly string userBodyTypePreferences = Path.Combine(userPreferencesDirectory, "bodytypepreferences.txt");
        private static readonly string userGenderPreferences = Path.Combine(userPreferencesDirectory, "genderpreferences.txt");
        private static readonly string userOrientationPreferences = Path.Combine(userPreferencesDirectory, "orientationpreferences.txt");
        private static readonly string userEthnicityPreferences = Path.Combine(userPreferencesDirectory, "ethnicitypreferences.txt");
        private static readonly string userRelationshipStatusPreferences = Path.Combine(userPreferencesDirectory, "relationshipstatuses.txt");

#warning these need to be private
        public static readonly string UsernamesToMessageFilePath = Path.Combine(ProgramDataDirectory, "usernames_to_message.txt");
        public static readonly string MessagesFilePath = Path.Combine(ProgramDataDirectory, "messages.txt");

        private static readonly string autheticationCookiesFilePath = Path.Combine(ProgramDataDirectory, "authenticationcookiedata.txt");

        private static string[] folderPaths = { ProfilePageDirectory, matchesDirectory, nonMatchesDirectory, QuestionsPageDirectory, bodyTypeExclusionsDirectory, userPreferencesDirectory };
        private static string[] filePaths = { userBodyTypePreferences, userGenderPreferences, userOrientationPreferences, userEthnicityPreferences
                , UsernamesToMessageFilePath, MessagesFilePath, processedProfilesPath, userRelationshipStatusPreferences, autheticationCookiesFilePath };

        // results files
        private static string MatchResultsFilePath = Path.Combine(matchesDirectory, "matches" + DateTime.Now.ToString("yyyy-M-dd") + ".txt");
        private static string NonMatchResultsFilePath = Path.Combine(nonMatchesDirectory, "nonmatches" + DateTime.Now.ToString("yyyy-M-dd") + ".txt");
        private static string BodyTypeExclusionsFilePath = Path.Combine(bodyTypeExclusionsDirectory, "bodytypeexclusions" + DateTime.Now.ToString("yyyy-M-dd") + ".txt");

        

        private static readonly TimeSpan TimeToKeepResultFiles = new TimeSpan(2, 0, 0, 0);

        private static Icebreaker programSettings = new Icebreaker();

        public static void Cleanup()
        {
            DirectoryInfo di = new DirectoryInfo(matchesDirectory);
            CleanupFiles(di);

            di = new DirectoryInfo(ProfilePageDirectory);
            CleanupFiles(di);

            di = new DirectoryInfo(nonMatchesDirectory);
            CleanupFiles(di);

            di = new DirectoryInfo(QuestionsPageDirectory);
            CleanupFiles(di);

            di = new DirectoryInfo(bodyTypeExclusionsDirectory);
            CleanupFiles(di);
        }

        public static HashSet<string> Initialize()
        {
            foreach(string path in folderPaths)
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            foreach(string path in filePaths)
            {
                if(!File.Exists(path))
                {
                    using (File.Create(path)) { };
                }
            }

            HashSet<string> processedProfiles = new HashSet<string>();
            processedProfiles = new HashSet<string>(File.ReadAllLines(processedProfilesPath));

            return processedProfiles;
        }

        public static SingleMessageSearch GetSingleMessageSearchRun()
        {
            SingleMessageSearch searchRun = new SingleMessageSearch();
            if (programSettings.NumMessagesToSend != 0)
            {
                IEnumerable<Gender> gendersSearchingFor = File.ReadAllLines(userGenderPreferences).Select(gs => (Gender)Enum.Parse(typeof(Gender), gs));
                IEnumerable<Orientation> orientationsSearchingFor = File.ReadAllLines(userOrientationPreferences).Select(os => (Orientation)Enum.Parse(typeof(Orientation), os));
                IEnumerable<BodyType> bodyTypesSearchingFor = File.ReadAllLines(userBodyTypePreferences).Select(bt => (BodyType)Enum.Parse(typeof(BodyType), bt));
                IEnumerable<Ethnicity> ethnicitiesSearchingFor = File.ReadAllLines(userEthnicityPreferences).Select(eth => (Ethnicity)Enum.Parse(typeof(Ethnicity), eth));
                IEnumerable<RelationshipStatus> relationshipStatuses = File.ReadAllLines(userRelationshipStatusPreferences).Select(rel => (RelationshipStatus)Enum.Parse(typeof(RelationshipStatus), rel));

                searchRun = new SingleMessageSearch(programSettings.MinAge
                    , programSettings.MaxAge
                    , programSettings.MinimumMatchPercentage
                    , programSettings.MaximumMatchPercentage
                    , programSettings.NumMessagesToSend
                    , programSettings.DefaultMessage
                    , programSettings.KidsAllowed
                    , bodyTypesSearchingFor
                    , ethnicitiesSearchingFor
                    , gendersSearchingFor
                    , orientationsSearchingFor
                    , relationshipStatuses);
            }

            return searchRun;
        }

        public static void SaveSingleMessageSearchRun(SingleMessageSearch searchRun)
        {
            programSettings.MinAge = searchRun.MinAge;
            programSettings.MaxAge = searchRun.MaxAge;
            programSettings.DefaultMessage = searchRun.MessageText;
            programSettings.MinimumMatchPercentage = searchRun.MinMatchPercentage;
            programSettings.MaximumMatchPercentage = searchRun.MaxMatchPercentage;
            programSettings.NumMessagesToSend = searchRun.NumMessagesToSend;
            programSettings.KidsAllowed = searchRun.KidsAllowed;
            File.WriteAllLines(userGenderPreferences, searchRun.GendersSearchingFor.Select(g => g.ToString()));
            File.WriteAllLines(userOrientationPreferences, searchRun.OrientationsSearchingFor.Select(o => o.ToString()));
            File.WriteAllLines(userBodyTypePreferences, searchRun.BodyTypesSearchingFor.Select(bt => bt.ToString()));
            File.WriteAllLines(userEthnicityPreferences, searchRun.EthnicitiesSearchingFor.Select(eth => eth.ToString()));
            File.WriteAllLines(userRelationshipStatusPreferences, searchRun.RelationshipStatuses.Select(rl => rl.ToString()));

            programSettings.Save();
        }

        public static CookieContainer GetAuthenticationCookies()
        {
            CookieContainer container = new CookieContainer();
            if (!File.Exists(autheticationCookiesFilePath))
                return container;

            using (Stream stream = File.Open(autheticationCookiesFilePath, FileMode.Open))
            {
                if (stream.Length > 0)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    container = (CookieContainer)formatter.Deserialize(stream);
                }
            }

            return container;
        }

        public static void SaveAuthenticationCookies(CookieContainer authenticationCookies)
        {
            using (Stream stream = File.OpenWrite(autheticationCookiesFilePath))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, authenticationCookies);
            }
        }

        /// <summary>
        /// Writes all the profile usernames in profileUsernames to the file containing
        /// all profile names that have been processed in previous runs of the application.
        /// </summary>
        /// <param name="profileUsernames"></param>
        public static void MarkProcessedProfiles(List<string> profileUsernames)
        {
            // remove messaged usernames from UsernamesToMessage file
            File.WriteAllLines(UsernamesToMessageFilePath, 
                File.ReadLines(UsernamesToMessageFilePath).Where(username => !profileUsernames.Contains(username)).ToList());

            File.WriteAllLines(processedProfilesPath, profileUsernames);
        }

        public static List<QuestionBasedMessage> GetQuestionBasedMessages()
        {
            return ParseMessageFile(MessagesFilePath);
        }

        public static List<string> UsernamesToMessageGet()
        {
            return File.ReadAllLines(UsernamesToMessageFilePath).ToList();
        }

        /// <summary>
        /// Constructs a filepath to the file used to contain data about
        /// the given username.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
#warning this method should be private
        public static string ProfilePageGetFilepath(string username)
        {
            return Path.Combine(ProfilePageDirectory, username);
        }

        /// <summary>
        /// Constructs a filepath to the file used to store the given
        /// username's answered questions for the page associated with
        /// the given questionNumberStep.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="questionNumberStep"></param>
        /// <returns></returns>
#warning this method should be private
        public static string QuestionsPageGetFilepath(string username, int questionNumberStep)
        {
            return Path.Combine(QuestionsPageDirectory, username + questionNumberStep.ToString());
        }

        public static void StoreMatchResult(string matchData, MatchStatus status)
        {
            switch (status)
            {
                case MatchStatus.ExcludedBodyType: WriteResultsToFile(BodyTypeExclusionsFilePath, matchData);
                    break;

                //case MatchStatus.InsufficientPhotos: WriteResultsToFile(

                case MatchStatus.MatchesSearchTerm: WriteResultsToFile(MatchResultsFilePath, matchData);
                    break;

                case MatchStatus.NoMatchingSearchTerm: WriteResultsToFile(NonMatchResultsFilePath, matchData);
                    break;
            }
        }

        /// <summary>
        /// Constructs Message objects based on the data found in the file
        /// at the path given by messageFilePath.
        /// </summary>
        /// <param name="messageFilePath"></param>
        /// <returns></returns>
        public static List<QuestionBasedMessage> ParseMessageFile(string messageFilePath)
        {
            List<QuestionBasedMessage> messages = new List<QuestionBasedMessage>();
            string[] messageDataLines = File.ReadAllLines(messageFilePath);

            foreach (string messageDataLine in messageDataLines)
            {
                string[] messageData = messageDataLine.Split(Constants.MessageDataSeparatorCharacter);
                messages.Add(new QuestionBasedMessage(Convert.ToInt32(messageData[0])
                                            , messageData[1]
                                            , messageData[2]
                                            , messageData[3]));
            }

            return messages;
        }

        public enum MatchStatus
        {
            InsufficientPhotos,
            ExcludedBodyType,
            MatchesSearchTerm,
            NoMatchingSearchTerm
        }

        private static void CleanupFiles(DirectoryInfo di)
        {
            foreach (FileInfo file in di.GetFiles().Where(file => DateTime.Now.Subtract(file.CreationTime) > TimeToKeepResultFiles))
            {
                file.Delete();
            }
        }

        /// <summary>
        /// If the a file at the given filePath exists this function
        /// appends all of fileContents to that file.  If the file does
        /// not exist then it creates it and writes all of fileContents
        /// to it.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileContents"></param>
        private static void WriteResultsToFile(string filePath, string fileContents)
        {
            if (!File.Exists(filePath))
                File.WriteAllText(filePath, fileContents);
            else
                File.AppendAllText(filePath, fileContents);
        }

    }


}
