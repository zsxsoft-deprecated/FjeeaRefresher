using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FjeeaRefresher
{
    static class NetworkOperator
    {
        public const string IndexUrl = "http://220.160.54.46/";
        public const string PostUrl = IndexUrl + "UEPORTLET/gkcj.shtml";
        public const string RandomImageUrl = IndexUrl + "UEPORTLET/radomImage";
        public const string LoginUrl = IndexUrl + "UEPORTLET/login.shtml";
        public static CookieContainer Cookies = new CookieContainer();


        public static string FormatUrlInHtml(string html)
        {
            Regex R = new Regex("(src|href|url)( ?= ?[\"']|\\()((?!http).*?)([\"']|\\))", RegexOptions.IgnoreCase);
            return "<script>window.alert = window.confirm = window.prompt = function(){};</script>" + R.Replace(html, "$1$2" + IndexUrl + "$3$4").Replace(IndexUrl + "/", IndexUrl);
            //return html.Replace("=\"/UEPORTLET/", $"=\"{IndexUrl}UEPORTLET/");
        }


        public static HttpClient InitializeHttp()
        {
            var HttpClientHeader = new HttpClientHandler
            {
                CookieContainer = Cookies,
            };
            var Ret = new HttpClient(HttpClientHeader);
            Ret.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2767.5 Safari/537.36");
            Ret.DefaultRequestHeaders.Referrer = new Uri("http://220.160.54.46/UEPORTLET/jsp/scores/gkcj/scores_enter.jsp");


            return Ret;
        }

        public static FormUrlEncodedContent EncodePostDataToHttpContent(Dictionary<string, string> body)
        {
            return new FormUrlEncodedContent(body.ToList());
        }

        public static async Task<string> GetRandomImage()
        {
            var Http = InitializeHttp();
            var Ret = await Http.GetAsync(RandomImageUrl);
            var ImageStream = await Ret.Content.ReadAsStreamAsync();
            var ImageBitmap = new Bitmap(ImageStream);
            string ValidationCode = ValidationCodeParser.GetVerifyCode(ImageBitmap);
            //ImageBitmap.Save("Z:\\" + ValidationCode + ".jpg");
            return ValidationCode;
        }

        public static async Task<string> PostToGKCJ()
        {
            var Http = InitializeHttp();
            var PostData = new Dictionary<string, string>();
            PostData.Add("check", await GetRandomImage());
            PostData.Add("method", "query");
            PostData.Add("logname", Config.Data.Username);
            PostData.Add("pwd", Encryption.Encrypt(Config.Data.Password));
            PostData.Add("ksh", Config.Data.Examinee);

            var Ret = await Http.PostAsync(PostUrl, EncodePostDataToHttpContent(PostData));
            return await new StreamReader(await Ret.Content.ReadAsStreamAsync(), Encoding.UTF8).ReadToEndAsync();
        }


        public static string GetSessionId()
        {
            var f = Cookies.List();
            return f["JSESSIONID"].Value.Replace("0000", "").Replace(":-1", "");
        }
        //        public static string GetSessionId() => Cookies.List()["JSESSIONID"].Value;

        public static async Task<string> TryLogin()
        {

            Console.WriteLine(Encryption.Encrypt(Config.Data.Password));
            var Http = InitializeHttp();
            var PostData = new Dictionary<string, string>();
            PostData.Add("check", await GetRandomImage());
            PostData.Add("method", "login");
            PostData.Add("secur", "1");
            PostData.Add("sessionid", GetSessionId());
            PostData.Add("loginName", Config.Data.Username);
            PostData.Add("loginPwd", Encryption.Encrypt(Config.Data.Password));
            var Ret = await Http.PostAsync(LoginUrl, EncodePostDataToHttpContent(PostData));
            return await new StreamReader(await Ret.Content.ReadAsStreamAsync(), Encoding.UTF8).ReadToEndAsync();
        }

        /// <summary>
        /// Lists the specified container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <see cref="http://stackoverflow.com/questions/13675154/how-to-get-cookies-info-inside-of-a-cookiecontainer-all-of-them-not-for-a-spe"/>
        /// <returns></returns>
        public static Dictionary<string, Cookie> List(this CookieContainer container)
        {
            var cookies = new Dictionary<string, Cookie>();
            var table = (Hashtable)container.GetType().InvokeMember("m_domainTable",
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.GetField |
                                                                    BindingFlags.Instance,
                                                                    null,
                                                                    container,
                                                                    new object[] { });
            foreach (var key in table.Keys)
            {
                Uri uri = null;
                var domain = key as string;
                if (domain == null)
                    continue;
                if (domain.StartsWith("."))
                    domain = domain.Substring(1);
                var address = string.Format("http://{0}/", domain);
                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out uri) == false)
                    continue;
                foreach (Cookie cookie in container.GetCookies(uri))
                {
                    cookies.Add(cookie.Name, cookie);
                }
            }
            return cookies;
        }
    }


}
