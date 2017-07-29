using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Icebreaker.Enums;

namespace Icebreaker
{
    public class UserInput
    {
        private string messagePrompt = "Type the message you would like to send and press enter:";
        private string usernamePrompt = "Enter your username and press enter: ";
        private string passwordPrompt = "Enter your password and press enter: ";
        private string minAgePrompt = "Enter the minimum age cutoff (18-100):";
        private string maxAgePrompt = "Enter the maximum age cutoff (18-100):";
        private string minMatchPrompt = "Enter the minimum match percentage cutoff (0-100):";
        private string maxMatchPrompt = "Enter the maximum match percentage cutoff (0-100):";
        private string numMessagesToSendPrompt = "Enter number of messages to send:";
        private string kidsAllowedPrompt = "Kids allowed (y/n)?";

        public SingleMessageSearch SingleMessageSearchRunCollect()
        {
            SingleMessageSearch searchRun = Storage.GetSingleMessageSearchRun();

            bool newRun = false;
            if (!string.IsNullOrEmpty(searchRun.MessageText))
            {
                bool validEntry = false;

                while (!validEntry)
                {

                    Console.WriteLine("Would you like to use your previous single message search run with the following data (y/n)?");

                    Console.WriteLine("Message: " + searchRun.MessageText);
                    Console.WriteLine("Number of messages to send: " + searchRun.NumMessagesToSend);
                    Console.WriteLine("Minimum match percentage: " + searchRun.MinMatchPercentage);
                    Console.WriteLine("Maximum match percentage: " + searchRun.MaxMatchPercentage);
                    Console.WriteLine("Minimum age: " + searchRun.MinAge);
                    Console.WriteLine("Maximum age: " + searchRun.MaxAge);
                    Console.WriteLine("Genders searching for: " + string.Join(",", searchRun.GendersSearchingFor.Select(g => g.ToString())));
                    Console.WriteLine("Body types searching for: " + string.Join(",", searchRun.BodyTypesSearchingFor.Select(bt => bt.ToString())));
                    Console.WriteLine("Ethnicities searching for: " + string.Join(",", searchRun.EthnicitiesSearchingFor.Select(bt => bt.ToString())));
                    Console.WriteLine("Relationship Statuses searching for: " + string.Join(",", searchRun.RelationshipStatuses.Select(rl => rl.ToString())));
                    Console.WriteLine("Kids allowed: " + searchRun.KidsAllowed.ToString());

                    string response = Console.ReadLine();

                    if (response == "y" || response == "n")
                    {
                        validEntry = true;
                        newRun = response == "n";
                    }
                    else
                    {
                        Console.WriteLine("Please enter only 'y' or 'n'.");
                    }
                }
            }
            else
            {
                newRun = true;
            }

            if(newRun)
            {
                Console.WriteLine("No prior single message search run detected - the following data is required.");

                string messageText = DefaultMessageCollect();

                int minAge = SingleIntegerCollect(minAgePrompt);
                int maxAge = 0;
                while (maxAge < minAge)
                    maxAge = SingleIntegerCollect(maxAgePrompt);

                int minMatch = SingleIntegerCollect(minMatchPrompt);
                int maxMatch = 0;
                while(maxMatch < minMatch)
                    maxMatch = SingleIntegerCollect(maxMatchPrompt);

                int numMessagesToSend = SingleIntegerCollect(numMessagesToSendPrompt);

                bool kidsAllowed = SingleBooleanCollect(kidsAllowedPrompt);

                IEnumerable<Gender> gendersSearchingFor = CollectEnumeration<Gender>();
                IEnumerable<Orientation> orientationsSearchingFor = CollectEnumeration<Orientation>();
                IEnumerable<BodyType> bodyTypesSearchingFor = CollectEnumeration<BodyType>();
                IEnumerable<Ethnicity> ethnicitiesSearchingFor = CollectEnumeration<Ethnicity>();
                IEnumerable<RelationshipStatus> relationshipStatuses = CollectEnumeration<RelationshipStatus>();

                searchRun = new SingleMessageSearch(minAge
                    , maxAge
                    , minMatch
                    , maxMatch
                    , numMessagesToSend
                    , messageText
                    , kidsAllowed
                    , bodyTypesSearchingFor
                    , ethnicitiesSearchingFor
                    , gendersSearchingFor
                    , orientationsSearchingFor
                    , relationshipStatuses);

                Storage.SaveSingleMessageSearchRun(searchRun);
            }

            return searchRun;
        }

        public string DefaultMessageCollect()
        {
            return NonEmptyStringCollect(messagePrompt);
        }

        public string UsernameCollect()
        {
            return NonEmptyStringCollect(usernamePrompt);
        }

        public string PasswordCollect()
        {
            return NonEmptyStringCollect(passwordPrompt);
        }

        public MessageRunType MessageRunTypeCollect()
        {
            Console.WriteLine("Enter the number of your desired run type and press enter:");

            foreach (Enums.MessageRunType rt in Enum.GetValues(typeof(MessageRunType)))
            {
                Console.WriteLine("{0} ({1})", rt.ToString(), Convert.ToInt32(rt).ToString());
            }
            MessageRunType runType = (MessageRunType)Convert.ToInt32(Console.ReadLine());

            return runType;
        }

        private IEnumerable<T> CollectEnumeration<T>() where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            List<T> enumCollection = new List<T>();

            Console.WriteLine();
            foreach (T enumEntry in EnumUtil.GetValues<T>())
            {
                string enumString = string.Format("{0} - {1}"
                    , ((Enum)(object)enumEntry).GetDescription()
                    , ((int)(object)enumEntry).ToString());
                Console.WriteLine(enumString);
            }

            bool validEntry = false;
            while (!validEntry)
            {
                Console.Write("Enter the numbers corresponding to the properties you want to message in a comma separated list \n(e.g. 1,2,4):");

                string userEntry = Console.ReadLine();
                string[] bodyTypeEntries = userEntry.Split(',');
                foreach (string bodyTypeEntry in bodyTypeEntries)
                {
                    T bodyType;
                    if (!Enum.TryParse(bodyTypeEntry, out bodyType))
                    {
                        Console.WriteLine(string.Format("Failed to parse entry {0} - please retry your entry.", bodyTypeEntry));
                        break;
                    }
                    else
                    {
                        enumCollection.Add(bodyType);
                    }
                }

                if (0 == enumCollection.Count)
                {
                    Console.WriteLine("No entries found - you must enter at least one property to message.");
                }
                else
                {
                    validEntry = true;
                }
            }

            return enumCollection;
        }

        private string NonEmptyStringCollect(string collectionMessage)
        {
            bool validEntry = false;
            string collectionText = string.Empty;
            while (!validEntry)
            {
                Console.WriteLine(collectionMessage);
                collectionText = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(collectionText))
                {
                    Console.WriteLine("Non-empty message is required.");
                }
                else
                {
                    validEntry = true;
                }
            }
            return collectionText;
        }

        private int SingleIntegerCollect(string collectionMessage)
        {
            int valueCollected = 0;

            Console.Write(collectionMessage);

            bool validEntry = false;
            while (!validEntry)
            {
                
                validEntry = int.TryParse(Console.ReadLine(), out valueCollected);

                if (!validEntry)
                    Console.WriteLine("Entry was invalid, please re-enter.");
            }

            return valueCollected;
        }

        private bool SingleBooleanCollect(string collectionMessage)
        {
            bool valueCollected = false;
            Console.WriteLine(collectionMessage);

            bool validEntry = false;
            while (!validEntry)
            {
                string entry = Console.ReadLine();
                validEntry = entry == "y" || entry == "n";

                if (!validEntry)
                {
                    Console.WriteLine("Entry was invalid, please re-enter.");
                }
                else
                {
                    valueCollected = entry == "y";
                }
            }

            return valueCollected;
        }
    }
}
