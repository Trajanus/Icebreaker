using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

namespace Icebreaker
{
    public class ProfilePage
    {
        private bool _isSearchable;
        private HtmlDocument _profileHtml;
        private MatchSummary.Datum _matchData;

        public bool IsSearchable
        {
            get { return _isSearchable; }
        }
        public string Username
        {
            get { return _matchData.username; }
        }

        public Uri ProfileUri
        {
            get { return ProfilePage.ProfilePageGetUri(Username); }
        }

        public ProfilePage(HtmlDocument pageHtml
                            , MatchSummary.Datum matchData)
        {
            _isSearchable = pageHtml != null && pageHtml.DocumentNode != null;
            _profileHtml = pageHtml;
            _matchData = matchData;
        }

        // Profiles with less photos, especially only one
        // photo are usually not real.
        public int PhotoCount()
        {
            return _profileHtml.DocumentNode.SelectNodes(Constants.XPathSelector.Photos).Count;
        }

        public bool HasBodyTypeSearchingFor(IEnumerable<Enums.BodyType> bodyTypesSearchingFor)
        {
            return ProfileSectionHasPropertySearchingFor(bodyTypesSearchingFor, Constants.XPathSelector.ProfileBasics);
        }

        public bool HasEthnicitySearchingFor(IEnumerable<Enums.Ethnicity> ethnicitiesSearchingFor)
        {
            return ProfileSectionHasPropertySearchingFor(ethnicitiesSearchingFor, Constants.XPathSelector.ProfileBackground);
        }

        public bool HasRelationshipStatus(IEnumerable<Enums.RelationshipStatus> relationshipStatusesSearchingFor)
        {
            return ProfileSectionHasPropertySearchingFor(relationshipStatusesSearchingFor, Constants.XPathSelector.ProfileBasics);
        }

        public bool HasKids()
        {
            return ProfileSectionHasPropertySearchingFor(Regex.Escape("Has Kid(s)"), Constants.XPathSelector.ProfileMisc);
        }

        private bool ProfileSectionHasPropertySearchingFor<T>(IEnumerable<T> propertiesSearchingFor, string profileSectionXPathSelector) where T : struct, IConvertible
        {
            string propertySearchRegex = string.Format("({0})", string.Join("|", propertiesSearchingFor));
            return ProfileSectionHasPropertySearchingFor(propertySearchRegex, profileSectionXPathSelector);
        }

        private bool ProfileSectionHasPropertySearchingFor(string regex, string profileSectionXPathSelector)
        {
            bool hasProperty = false;

            try
            {
                foreach (HtmlNode profilePageBasics in _profileHtml.DocumentNode.SelectNodes(profileSectionXPathSelector))
                {
                    string profilePageBasicsText = profilePageBasics.InnerText.ToLower();
                    hasProperty = Regex.IsMatch(profilePageBasicsText, regex, RegexOptions.IgnoreCase);
                    if (hasProperty)
                    {
                        break;
                    }
                }
            }
            catch
            {
                //Console.WriteLine(ProfileUri.ToString());
                //throw;
            }

            return hasProperty;
        }

        public bool MatchesSearchTerm(string[] termsList, string termsRegex, out string termsMatched)
        {
            bool isMatch = false;
            termsMatched = "";
            HtmlNodeCollection profileHtmlNodes = _profileHtml.DocumentNode.SelectNodes(Constants.XPathSelector.ProfileInfo);

            if (null != profileHtmlNodes) // some users do not write anything in their profiles
            {

                bool profileSectionIsMatch = false;
                string profilePageText;
                termsMatched = "";

                foreach (HtmlNode profilePageUserData in profileHtmlNodes)
                {
                    profilePageText = profilePageUserData.InnerText.ToLower();
                    profileSectionIsMatch = Regex.IsMatch(profilePageText, termsRegex);

                    if (profileSectionIsMatch)
                    {
                        isMatch = true;
                        foreach (string searchTerm in termsList)
                        {
                            if (Regex.IsMatch(profilePageText, searchTerm))
                            {
                                termsMatched += searchTerm + " ";
                            }
                        }
                    }
                }
            }

            return isMatch;
        }

        public static Uri ProfilePageGetUri(string username)
        {
            string urlPath = Path.Combine(Constants.OkcUri.ProfilePageUrlPart + username);
            return new Uri(Constants.OkcUri.OkcUrl, urlPath);
        }

    }
}
