using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace CrawlerPrototype
{
    class HttpMethods
    {
        public static string _key = "Lütfen bilgilerinizi kontrol ediniz!";
        public static CookieContainer cookies = new CookieContainer();

        /// <summary>
        /// The 'GET' method
        /// </summary>
        /// <param name="url">Website URL</param>
        /// <param name="referer">Referer URL</param>
        /// <param name="cookies">CookieContainer object</param>
        /// <returns></returns>
        public static string Get(string url, string referer, ref CookieContainer cookies)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.CookieContainer = cookies;
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.157 Safari/537.36";
            req.Referer = referer;

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            cookies.Add(resp.Cookies);

            string pageSrc;
            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
            {
                pageSrc = sr.ReadToEnd();
            }

            return pageSrc;
        }
        public static bool Post(string url, string referer, string postData, CookieContainer cookies)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.CookieContainer = cookies;
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.157 Safari/537.36";
            req.Referer = referer;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";

            byte[] postBytes = Encoding.ASCII.GetBytes(postData);
            req.ContentLength = postBytes.Length;
            using (Stream postStream = req.GetRequestStream())
            {
                postStream.Write(postBytes, 0, postBytes.Length);
            }

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            cookies.Add(resp.Cookies);

            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string pageSrc = sr.ReadToEnd();
            sr.Dispose();

            return (!pageSrc.Contains(_key));
        }
        public static string GetBetween(string source, string begin, string end)
        {
            int startIndex = source.IndexOf(begin) + begin.Length;
            source = source.Substring(startIndex);
            int endIndex = source.IndexOf(end);
            source = source.Substring(0, endIndex);
            return source;
        }
    }
}
