using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Icebreaker
{
    public class Constants
    {
        public static readonly Regex AuthcodeRegex = new Regex("var AUTHCODE = \".*\"");

        public const char MessageDataSeparatorCharacter = '|';

        public const int QuestionsPerPageStepValue = 10; // the value by which the 'low' query string parameter to the questions pages increments.

        public static class OkcUri
        {
            public static readonly Uri OkcUrl = new Uri("https://www.okcupid.com/");
            public static readonly Uri HomePage = new Uri("https://www.okcupid.com/home");
            public static readonly Uri ProfileHomepage = new Uri("https://www.okcupid.com/profile");
            public static readonly Uri LoginUri = new Uri("https://www.okcupid.com/login");
            public static readonly Uri SendMessageUrl = new Uri("https://www.okcupid.com/1/apitun/messages/send");

            public const string MatchPageUrlPart = "match";
            public const string ProfilePageUrlPart = "profile/";
            public const string QuestionsUrlPart = "/questions?n=1&i_care=1&low=";
        }

        public static class XPathSelector
        {
            public const string ProfileInfo = "//div[contains(@class,'essays2015-essay-content')]";
            public const string ProfileBasics = "//table[contains(@class,'basics') and contains(@class,'details2015-section')]";
            public const string ProfileBackground = "//table[contains(@class,'background') and contains(@class,'details2015-section')]";
            public const string ProfileMisc = "//table[contains(@class,'misc') and contains(@class,'details2015-section')]";

            public const string AnswerHtmlId = "//span[@id = 'answer_target_{0}']"; // this requires the numeric question id to be appended to it

            // profile sections
            public const string XPathSelector_SelfSummary = "//div[contains(@class,'essays2015-essay-title') and text()=' My self-summary ']/following-sibling::div";
            public const string XPathSelector_DoingWithLife = "//div[contains(@class,'essays2015-essay-title') and text()=' What I’m doing with my life ']/following-sibling::div";
            public const string XPathSelector_ReallyGoodAt = "//div[contains(@class,'essays2015-essay-title') and text()=' I’m really good at ']/following-sibling::div";
            public const string XPathSelector_FavoriteStuff = "//div[contains(@class,'essays2015-essay-title') and text()=' Favorite books, movies, shows, music, and food ']/following-sibling::div";
            public const string XPathSelector_Necessities = "//div[contains(@class,'essays2015-essay-title') and text()=' The six things I could never do without ']/following-sibling::div";
            public const string XPathSelector_ThinksAbout = "//div[contains(@class,'essays2015-essay-title') and text()=' I spend a lot of time thinking about ']/following-sibling::div";
            public const string XPathSelector_FridayNight = "//div[contains(@class,'essays2015-essay-title') and text()=' On a typical Friday night I am ']/following-sibling::div";
            public const string XPathSelector_MessageMeIf = "//div[contains(@class,'essays2015-essay-title') and text()=' You should message me if ']/following-sibling::div";

            // photo related
            public const string Photos = "//div[@class='userinfo2015-thumb']/child::*";

            // questions
            public const string NextQuestionsLinkDisabled = "//li[contains(@class,'next') and contains(@class,'disabled')]/descendant::a[1]";
            public const string NextQuestionsLink = "//li[contains(@class,'next')]/descendant::a[1]";
        }
        
        // http related
        public static class Http
        {
            public const string AcceptHeaderDefault = "text/html";
            public const string AcceptHeader_Login = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            public const string AcceptHeaderHome = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            public const string ParamNameUsername = "username";
            public const string ParamNamePassword = "password";
            public const string ParamNameOkCupidApi = "okc_api";

            public const string ContentTypeHttpQuery = "application/x-www-form-urlencoded; charset=UTF-8";
            public const string ContentTypeJson = "application/json; charset=UTF-8";
        }

        // app.config keys
        public static class AppConfigKeys
        {
            public const string CleanupFiles = "CleanupFiles";
            public const string MatchSummaryJsonStartPattern = "matchSummaryJSONStartPattern";
            public const string MatchSummaryJsonEndPattern = "matchSummaryJSONEndPattern";
            public const string UserAgent = "UserAgent";
            public const string SendMessages = "SendMessages";
            public const string Username = "Username";
            public const string Password = "Password";
        }
    }
}
