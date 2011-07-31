using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Web;
using System.Xml;
using System.IO;
using System.Windows;
using System.Text.RegularExpressions;

namespace SilhouetteSaver
{
    class Niconico
    {
        private const string URL_LOGIN = @"https://secure.nicovideo.jp/secure/login?site=niconico";
        private const string REGEX_SM_URL = @"^http://www.nicovideo.jp/watch/sm(?<videoid>[0-9]+)$";
        private const string URL_GETFLV = @"http://www.nicovideo.jp/api/getflv/sm";

        public Niconico()
        {
            _getFlvRegex = new Regex(REGEX_SM_URL, RegexOptions.Compiled);
        }

        private Regex _getFlvRegex;

        private bool _isLogin = false;
        public bool IsLogin
        {
            get { return _isLogin; }
        }

        private CookieContainer _cookie = null;

        public bool Login(string userName, string passWord)
        {
            Dictionary<string, string> hash = new Dictionary<string, string>(3);
            hash.Add("mail", userName);
            hash.Add("password", passWord);
            hash.Add("next_url", null);

            _cookie = new CookieContainer();

            string result = httpPost(URL_LOGIN, hash, _cookie);
            if (string.IsNullOrEmpty(result) ||
                result.IndexOf("ログインエラー") >= 0 || result.IndexOf("メールアドレスまたはパスワードが間違って") >= 0)
            {
                _cookie = null;
                _isLogin = false;
                return false;
            }

            _isLogin = true;
            return true;
        }

        private string httpPost(string url, string paramStr, CookieContainer cookie)
        {
            byte[] data = Encoding.ASCII.GetBytes(paramStr);
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.CookieContainer = cookie;
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            req.Method = "Post";

            try
            {
                Stream reqStream = req.GetRequestStream();
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }

            WebResponse res = req.GetResponse();
            Stream resStream = res.GetResponseStream();
            StreamReader sr = new StreamReader(resStream, Encoding.UTF8);
            try
            {
                string result = sr.ReadToEnd();
                return result;
            }
            catch (Exception ex)
            {
                res.Close();
                resStream.Close();
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private string httpPost(string url, Dictionary<string, string> param, CookieContainer cookie)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in param)
            {
                sb.Append(string.Format("{0}={1}&", kv.Key, kv.Value));
            }
            string paramStr = sb.ToString();

            return httpPost(url, paramStr, cookie);
        }

        private string httpGet(string url, CookieContainer cookie)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.CookieContainer = cookie;
            req.AllowAutoRedirect = true;

            WebResponse res = req.GetResponse();
            Stream resStream = res.GetResponseStream();
            using (StreamReader sr = new StreamReader(resStream))
            {
                string result = sr.ReadToEnd();
                return result;
            }
        }

        public Dictionary<string, string> GetVideoInfo(string url)
        {
            if (!IsLogin) return null;

            if (_cookie == null) return null;

            string vid = "";
            Match matchUrl = _getFlvRegex.Match(url);
            if (matchUrl.Success)
            {
                vid = matchUrl.Groups["videoid"].ToString();
            }
            else
            {
                return null;
            }

            string getflvUrl = URL_GETFLV + vid;

            string response = httpGet(getflvUrl, _cookie);

            string dec = Uri.EscapeUriString(response); // only utf-8
            Dictionary<string, string> hash = new Dictionary<string, string>();
            string[] pairs = dec.Split(new char[] { '&' });
            foreach (string s in pairs)
            {
                string[] keyValue = s.Split(new char[] { '=' });
                if (keyValue.Length != 2) continue;
                if (!hash.ContainsKey(keyValue[0]))
                    hash.Add(keyValue[0], keyValue[1]);
            }
            return hash;
        }
    }
}
