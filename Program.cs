using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TeamsGraph
{
    public class MSUserInfo
    {
        public string UniqueId { get; }

        public string DisplayableId { get; }

        public string GivenName { get; }

        public string FamilyName { get; }

        public string IdentityProvider { get; }

        public string TenantId { get; }
    }

    class Program
    {
        public static void Main()
        {
            var authorize = "https://login.microsoftonline.com/common/oauth2/authorize?resource=https%3A%2F%2Frohanpvtlimited.sharepoint.com&client_id=2bae50d2-90aa-4b11-aca4-3caf162934f9&response_type=code&redirect_uri=http%3A%2F%2Flocalhost";

            var res = GetResponse(authorize);

            GraphClient graphClient = new GraphClient(new GraphClientConfig());
            var s = graphClient.GetMyTeams(new Connection() { UserName = "" });
            s.Wait();
        }


        public static void JSONSer()
        {
            string cacheFolder = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"..\..\..\..");
            string adalV3cacheFileName = Path.Combine(cacheFolder, "cacheAdalV3.bin");
            string msalCacheFileName = Path.Combine(cacheFolder, "cacheMsal.bin");

            FilesBasedTokenCache tokenCache = new FilesBasedTokenCache(adalV3cacheFileName, msalCacheFileName);
            var res = JsonConvert.DeserializeObject(tokenCache.ReadFromFileIfExists(msalCacheFileName).ToString(), typeof(MSUserInfo));
            Console.ReadKey();
        }

        private static string GetResponse(string resourceUrl)
        {
            HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(resourceUrl);
            wreq.Method = "GET";
            string result;
            try
            {
                var clientId = "2bae50d2-90aa-4b11-aca4-3caf162934f9";
                HttpWebResponse wresp = wreq.GetResponse() as HttpWebResponse;
                using (StreamReader sr = new StreamReader(wresp.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(result);
                var asdkjlks = htmlDoc.DocumentNode.Descendants("script").ToArray().FirstOrDefault(s => s.InnerText.Contains("$Config={"));

                var asd = asdkjlks.InnerText.Replace("//<![CDATA[", "").Replace("//]]>", "").Trim().Replace("$Config=", "").Trim(';').Trim();

                var jss = new JavaScriptSerializer();
                var val = jss.Deserialize<Dictionary<string, object>>(asd);
                var ctx = val["sCtx"] as string;
                var flowToken = val["sFT"] as string;

                var login = "https://login.microsoftonline.com/common/login";
                HttpWebRequest wreq2 = (HttpWebRequest)WebRequest.Create(login);
                wreq2.Method = "POST";
                wreq2.ContentType = "application/x-www-form-urlencoded";
                for (int i = 0; i < wresp.Headers.Count; i++)
                {
                    string name = wresp.Headers.GetKey(i);
                    if (name != "Set-Cookie")
                        continue;
                    string value = wresp.Headers.Get(i);
                    foreach (var singleCookie in value.Split(','))
                    {
                        try
                        {
                            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(singleCookie, "(.+?)=(.+?);");
                            if (match.Captures.Count == 0)
                                continue;
                            wresp.Cookies.Add(
                                new Cookie(
                                    match.Groups[1].ToString(),
                                    match.Groups[2].ToString(),
                                    "/",
                                    wreq.Host.Split(':')[0]));
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                wreq2.CookieContainer = new CookieContainer();
                wreq2.CookieContainer.Add(wresp.Cookies);
                using (StreamWriter sw = new StreamWriter(wreq2.GetRequestStream()))
                {
                    sw.Write($"login=rohan.p%40rohanpvtlimited.onmicrosoft.com&passwd=Guddu%4097p&ctx={ctx}&flowToken={flowToken}");
                }
                string responseString = null;
                var response = wreq2.GetResponse() as HttpWebResponse;
                if (response == null) return null;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    responseString = reader.ReadToEnd();
                }


                HtmlDocument htmlDoc2 = new HtmlDocument();
                htmlDoc2.LoadHtml(responseString);
                var asdkjlks2 = htmlDoc2.DocumentNode.Descendants("script").ToArray().FirstOrDefault(s => s.InnerText.Contains("$Config={"));

                var asd2 = asdkjlks2.InnerText.Replace("//<![CDATA[", "").Replace("//]]>", "").Trim().Replace("$Config=", "").Trim(';').Trim();

                var jss2 = new JavaScriptSerializer();
                var val2 = jss2.Deserialize<Dictionary<string, object>>(asd2);
                var ctx2 = val2["sCtx"] as string;
                var flowToken2 = val2["sFT"] as string;

                var grant = "https://login.microsoftonline.com/common/Consent/Grant";
                HttpWebRequest wreq3 = (HttpWebRequest)WebRequest.Create(grant);
                wreq3.Method = "POST";
                wreq3.ContentType = "application/x-www-form-urlencoded";
                for (int i = 0; i < response.Headers.Count; i++)
                {
                    string name = response.Headers.GetKey(i);
                    if (name != "Set-Cookie")
                        continue;
                    string value = response.Headers.Get(i);
                    foreach (var singleCookie in value.Split(','))
                    {
                        try
                        {
                            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(singleCookie, "(.+?)=(.+?);");
                            if (match.Captures.Count == 0)
                                continue;
                            wresp.Cookies.Add(
                                new Cookie(
                                    match.Groups[1].ToString(),
                                    match.Groups[2].ToString(),
                                    "/",
                                    wreq2.Host.Split(':')[0]));
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                wreq3.CookieContainer = new CookieContainer();
                wreq3.CookieContainer.Add(wresp.Cookies);
                using (StreamWriter sw = new StreamWriter(wreq3.GetRequestStream()))
                {
                    sw.Write($"ctx={ctx2}&flowToken={flowToken2}&upgradeToAdminConsent=true");
                }

                var response2 = wreq3.GetResponse() as HttpWebResponse;
                if (response2 == null) return null;
                string responseString2 = null;
                using (StreamReader reader = new StreamReader(response2.GetResponseStream()))
                {
                    responseString2 = reader.ReadToEnd();
                }
                var code = System.Web.HttpUtility.ParseQueryString(response2.ResponseUri.Query).Get("code");


                var tokenUrl = "https://login.microsoftonline.com/common/oauth2/token";
                HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenUrl);
                tokenRequest.Method = "POST";
                tokenRequest.ContentType = "application/x-www-form-urlencoded";
                
                using (StreamWriter sw = new StreamWriter(tokenRequest.GetRequestStream()))
                {
                    sw.Write($"grant_type=authorization_code&code={code}&client_id={clientId}&redirect_uri=http%3A%2F%2Flocalhost&resource=https%3A%2F%2Frohanpvtlimited.sharepoint.com");
                }

                var response3 = tokenRequest.GetResponse() as HttpWebResponse;
                if (response3 == null) return null;
                string responseString3 = null;
                using (StreamReader reader = new StreamReader(response3.GetResponseStream()))
                {
                    responseString3 = reader.ReadToEnd();
                }

                return null;



            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
}
