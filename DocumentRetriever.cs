using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

using HtmlAgilityPack;

namespace Icebreaker
{
    public class DocumentRetriever
    {
        private CookieContainer _authCookies;
        private string _userAgent;
        
        private bool _sendMessages;

        private string _baseUrl;

        // These are the cookies that okcupid requires in a request
        // to authenticate as a user.
        public CookieContainer AuthCookies
        {
            get
            {
                return _authCookies;
            }
            set
            {
                _authCookies = value;
            }
        }

        protected bool SendMessages
        {
            get { return _sendMessages; }
            set { _sendMessages = value; }
        }

        public DocumentRetriever(string baseUrl
            , string userAgent
            , bool sendMessages)
        {
            _baseUrl = baseUrl;
            _userAgent = userAgent;
            _sendMessages = sendMessages;
        }

        public HtmlDocument RetrieveHtmlDocument(string url
                                                , string acceptHttpHeader)
        {
            string pageResults;

            HttpWebResponse response = GetWebResponse(url, acceptHttpHeader, AuthCookies);

            using (Stream dataStream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    pageResults = reader.ReadToEnd();

                    HtmlAgilityPack.HtmlDocument pageHTML = new HtmlAgilityPack.HtmlDocument();
                    pageHTML.LoadHtml(pageResults);

                    return pageHTML;
                }
            }
        }

        public bool SendOkCupidMessage(string url
            , string acceptHttpHeader
            , string contentType
            , string postData
            , string authCode)
        {
            bool sendMessageSuccess = false;
            if (SendMessages)
            {
                HttpWebResponse response = SendPost(url
                    , acceptHttpHeader
                    , contentType
                    , postData
                    , AuthCookies
                    , authCode);

                sendMessageSuccess = response.StatusCode == HttpStatusCode.OK;
            }

            return sendMessageSuccess;
        }

        public HttpWebResponse SendPost(string url
            , string acceptHttpHeader
            , string contentType
            , string postData
            , CookieContainer urlCookies = null
            , string authCode = "")
        {
            HttpWebRequest request = (HttpWebRequest)
            WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Accept = acceptHttpHeader;
            request.UserAgent = _userAgent;
            request.Method = "POST";

            if(string.Empty != authCode)
                request.Headers.Add(string.Format("Authorization: Bearer {0}", authCode));

            request.CookieContainer = null == urlCookies ? new CookieContainer() : urlCookies;

            var data = Encoding.UTF8.GetBytes(postData);

            request.ContentType = contentType;
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            return (HttpWebResponse)request.GetResponse();
        }

        /// <summary>
        /// Returns CookieContainer with cookies necessary for authentication
        /// with okcupid.com
        /// </summary>
        /// <returns></returns>
        public CookieContainer GetAuthCookieList(string username, string password)
        {
            CookieContainer okCupidCookies = new CookieContainer();

            // This post to the login page gets the authlink cookie which is enough to navigate the okc site
            // but not enough to send messages.
            string postData = String.Format("username={0}&password={1}&okc_api=1", username, password);

            HttpWebResponse loginResponse = SendPost(Constants.OkcUri.LoginUri.ToString()
                                                        , Constants.Http.AcceptHeader_Login
                                                        , Constants.Http.ContentTypeHttpQuery
                                                        , postData
                                                        , okCupidCookies);

            foreach (Cookie cookie in loginResponse.Cookies)
            {
                if("guest" != cookie.Name)
                    okCupidCookies.Add(cookie);
            }

            loginResponse.Close();

            HttpWebResponse loginGet = GetWebResponse(Constants.OkcUri.LoginUri.ToString()
                                                        , Constants.Http.AcceptHeader_Login
                                                        , okCupidCookies);

            foreach (Cookie cookie in loginGet.Cookies)
            {
                //if (!okCupidCookies.Exists(c => c.Name == cookie.Name))
                    okCupidCookies.Add(cookie);
            }

            loginGet.Close();

            HttpWebResponse homeResponse = GetWebResponse(Constants.OkcUri.HomePage.ToString()
                                                            , Constants.Http.AcceptHeaderHome
                                                            , okCupidCookies);

            foreach (Cookie cookie in homeResponse.Cookies)
            {
                //if (!okCupidCookies.Exists(c => c.Name == cookie.Name))
                    okCupidCookies.Add(cookie);
            }

            //CookieContainer cookieContainer = new CookieContainer();
            //Uri requestUri = new Uri(_baseUrl);
            //string UriAuthority = requestUri.GetLeftPart(UriPartial.Authority);

            //foreach (Cookie urlCookie in okCupidCookies)
            //{
            //    cookieContainer.Add(new Uri(UriAuthority), urlCookie);
            //}

            return okCupidCookies;
        }

        private HttpWebResponse GetWebResponse(string url
                                                , string acceptHttpHeader
                                                , CookieContainer urlCookies = null)
        {
            HttpWebRequest request = (HttpWebRequest)
            WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Accept = acceptHttpHeader;
            request.UserAgent = _userAgent;
            request.CookieContainer = null == urlCookies ? new CookieContainer() : urlCookies;

            return (HttpWebResponse)request.GetResponse();
        }
    }
}
