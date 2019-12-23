using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.SharePoint.Client;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Web.Services.Protocols;
using TeamsGraph.SitesWebService;
using AzureFunctionsForSharePoint.Common;

namespace TeamsGraph
{
    public class GraphClient
    {
        public GraphClientConfig GraphClientConfig { get; set; }

        public GraphClient(GraphClientConfig graphClientConfig)
        {
            GraphClientConfig = graphClientConfig;
        }

        /// <summary>
        ///  Creates a new GraphServiceClient instance using a custom PnPHttpProvider
        /// </summary>
        /// <param name="accessToken">The OAuth 2.0 Access Token to configure the HTTP bearer Authorization Header</param>
        /// <returns></returns>
        public GraphServiceClient CreateGraphClient(string accessToken)
        {
            return new GraphServiceClient(new DelegateAuthenticationProvider(
                async (requestMessage) =>
                {
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        requestMessage.Headers.Add("Accept", "application/json, text/plain, */*");
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                        requestMessage.Headers.Add("AllowAutoRedirect", "true");
                    }
                }));
        }

        public async Task<string> GetCurrentUser(Connection connection)
        {
            string[] scopes = { "Files.Read", "Files.ReadWrite", "Files.Read.All", "Files.ReadWrite.All", "Sites.Read.All", "Sites.ReadWrite.All" };

            var accessToken = await GraphClientConfig.GetTokenForUserAsync(connection, scopes);
            var graphClient = CreateGraphClient(accessToken);
            var currentUser = await graphClient.Me.Request().GetAsync();

            return null;
        }
        public async Task<string> CreateTeam(Connection connection)
        {
            string[] scopes = { "Group.ReadWrite.All", "Directory.ReadWrite.All", "Directory.AccessAsUser.All" };
            var accessToken = await GraphClientConfig.GetTokenForUserAsync(connection, scopes);
            var graphClient = CreateGraphClient(accessToken);

            var team = new Team
            {
                MemberSettings = new TeamMemberSettings
                {
                    AllowCreateUpdateChannels = true,
                    ODataType = null
                },
                MessagingSettings = new TeamMessagingSettings
                {
                    AllowUserEditMessages = true,
                    AllowUserDeleteMessages = true,
                    ODataType = null
                },
                FunSettings = new TeamFunSettings
                {
                    AllowGiphy = true,
                    GiphyContentRating = GiphyRatingType.Strict,
                    ODataType = null
                },
                ODataType = null
            };
            await graphClient.Groups["780800ec-b6f6-4ca5-b48e-5249d0685d04"].Team
    .Request()
    .PutAsync(team);
            return null;
        }

        public async Task<string> CreateGroup(Connection connection)
        {
            string[] scopes = { "Group.ReadWrite.All", "Directory.ReadWrite.All", "Directory.AccessAsUser.All" };
            var accessToken = await GraphClientConfig.GetTokenForUserAsync(connection, scopes);
            var graphClient = CreateGraphClient(accessToken);
            //var groups = await graphClient.Groups.Request().GetAsync();
            var group = new Microsoft.Graph.Group
            {
                DisplayName = "test team 123",
                Description = "Created using Graph API",
                // Mail = "Graph@spmig.onmicrosoft.com",
                MailEnabled = true,
                MailNickname = "testteam1",
                GroupTypes = new List<string> { "Unified" },
                Visibility = "private",
                SecurityEnabled = false
            };
            await graphClient.Groups
    .Request()
    .AddAsync(group);
            return null;
        }

        public async Task<string> GetGroups(Connection connection)
        {
            string[] scopes = { "Group.ReadWrite.All", "Directory.ReadWrite.All", "Directory.AccessAsUser.All" };
            var accessToken = await GraphClientConfig.GetTokenForUserAsync(new Connection
            {
                UserName = "spmigrator@technovertdeveloper.onmicrosoft.com",
                Password = "saketa@123"
            }, scopes);
            var graphClient = CreateGraphClient(accessToken);
            var groups = await graphClient.Groups.Request().GetAsync();
            foreach (var group in groups)
            {
                try
                {
                    var sites = await graphClient.Groups[$"{group.Id}"].Sites["root"]
.Request()
.Select(e => new
{
    e.WebUrl
}).GetAsync();
                    Console.WriteLine($"{group.Visibility}--{group.DisplayName}----success");
                }
                catch (Exception)
                {
                    Console.WriteLine($"{group.Visibility}--{group.DisplayName}");
                }

            }


            return null;
        }
        //"780800ec-b6f6-4ca5-b48e-5249d0685d04"

        public async Task<string> GetMyTeams(Connection connection)
        {
            //string[] scopes = { "Sites.Read.All", "Sites.ReadWrite.All", "Sites.FullControl.All", "Sites.Manage.All" };
            //var accessToken = await GraphClientConfig.GetTokenForUserAsync(connection, scopes);
            var s = GetFormDigestFromWebService(@"https://rohanpvtlimited.sharepoint.com/");



            Console.WriteLine(s);
            // var graphClient = CreateGraphClient(accessToken);
            //        var joinedTeams = await graphClient.Me.JoinedTeams
            //.Request()
            //.GetAsync();

            return null;
        }
        //"55485f98-ff30-4a56-b43b-e9cd09422f9b" --test
        public async Task<string> GetTeam(Connection connection)
        {
            string[] scopes = { "User.Read.All", "User.ReadWrite.All" };
            var accessToken = await GraphClientConfig.GetTokenForUserAsync(connection, scopes);
            var graphClient = CreateGraphClient(accessToken);
            var joinedTeams = await graphClient.Teams["55485f98-ff30-4a56-b43b-e9cd09422f9b"].Request().GetAsync();
            //GetContext(accessToken);
            return null;
        }
        public async Task<string> CreateChannel(Connection connection)
        {
            string[] scopes = { "Group.ReadWrite.All", "Directory.ReadWrite.All", "Directory.AccessAsUser.All" };
            var accessToken = await GraphClientConfig.GetTokenForUserAsync(connection, scopes);
            var graphClient = CreateGraphClient(accessToken);
            List<Task> tasks = new List<Task>();
            List<Channel> channels = new List<Channel>();
            for (int i = 0; i < 20; i++)
            {
                var channel = new Channel
                {
                    DisplayName = $"Graph Channel{i}00",
                    Description = "This is create by using graph api"
                };
                channels.Add(channel);
            }
            foreach (var ch in channels)
            {
                var task = Task.Run(async () => await graphClient.Teams["142f9c83-4bf4-4b85-a1f4-48948bb8f5bc"].Channels
.Request()
.AddAsync(ch));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
            return null;
        }

        public async Task<string> GetChannels(Connection connection)
        {//"142f9c83-4bf4-4b85-a1f4-48948bb8f5bc"
            string[] scopes = { "Group.Read.All", "Group.ReadWrite.All" };
            var accessToken = await GraphClientConfig.GetTokenForUserAsync(connection, scopes);
            var graphClient = CreateGraphClient(accessToken);

            var channels = await graphClient.Teams["142f9c83-4bf4-4b85-a1f4-48948bb8f5bc"].Channels
    .Request()
    .GetAsync();

            //var group = await graphClient.Groups["142f9c83-4bf4-4b85-a1f4-48948bb8f5bc"].Request().GetAsync();
            return null;
        }

        public async Task<string> ValidateGroup(Connection connection)
        {
            string[] scopes = { "User.Read", "User.ReadWrite", "User.ReadBasic.All", "User.Read.All", "User.ReadWrite.All", "Directory.Read.All", "Directory.ReadWrite.All", "Directory.AccessAsUser.All" };
            var accessToken = await GraphClientConfig.GetTokenForUserAsync(connection, scopes);
            var graphClient = CreateGraphClient(accessToken);
            var user = await graphClient.Me
    .Request()
    .GetAsync();
            await graphClient.DirectoryObjects
                .ValidateProperties("Group", "test", "testteam1", Guid.Parse(user.Id))
                .Request()
                .PostAsync();
            return null;

        }

        public void GetContext(string accessToken)
        {
            ac = accessToken;
            ClientContext cc = new ClientContext(@"https://spmig.sharepoint.com");
            cc.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>(context_ExecutingWebRequest);
            cc.Load(cc.Web, p => p.Title);
            cc.ExecuteQuery();

            Console.WriteLine(cc.Web.Title);

        }
        public static string ac { get; set; }
        static void context_ExecutingWebRequest(object sender, WebRequestEventArgs e)
        {
            e.WebRequestExecutor.WebRequest.Headers["Authorization"] = string.Format("Bearer {0}", ac);
        }



        private string GetFormDigest(string siteUrl, string accessToken)
        {
            string resourceUrl = $"{siteUrl}_api/contextinfo";
            HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(resourceUrl);
            wreq.Method = "GET";
            wreq.Headers.Add("Authorization", $"Bearer {accessToken}");
            wreq.Accept = "*/*";
            string result;
            try
            {
                WebResponse wresp = wreq.GetResponse();
                using (StreamReader sr = new StreamReader(wresp.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
                var jss = new JavaScriptSerializer();
                var val = jss.Deserialize<Dictionary<string, object>>(result);
                var d = val["d"] as Dictionary<string, object>;
                var wi = d["GetContextWebInformation"] as Dictionary<string, object>;
                var formDigest = wi["FormDigestValue"].ToString();
                return formDigest;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public TWsType CreateWebService<TWsType>() where TWsType : SoapHttpClientProtocol, new()
        {
            TWsType webService = new TWsType();
            string webServiceUrl = $"https://rohanpvtlimited.sharepoint.com/_vti_bin/";

            webService.Url = string.Format("{0}{1}.asmx", webServiceUrl, "Sites");
            webService.PreAuthenticate = false;
            webService.EnableDecompression = true;
            webService.UnsafeAuthenticatedConnectionSharing = true;
            // webService.Proxy = null;
            webService.UserAgent = $"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0; MS FrontPage; Saketa; ISV | Technovert | SaketaMigrator /2.4.2)";
            return webService;
        }

        public string GetFormDigestFromWebService(string siteUrl)
        {
            try
            {
                using (SKSitesWebService objSite = CreateWebService<SKSitesWebService>())
                {
                    FormDigestInformation formDigest = objSite.GetUpdatedFormDigestInformation(siteUrl);
                    var site = objSite.GetSite(siteUrl);
                    return formDigest.DigestValue;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
    public class SKSitesWebService : TeamsGraph.SitesWebService.Sites
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            var request = base.GetWebRequest(uri);
            //Add the Accept-Language header (for Danish) in the request.
            string[] scopes = { "Sites.Read.All", "Sites.ReadWrite.All", "Sites.FullControl.All", "Sites.Manage.All", "Mail.Send", "Reports.Read.All", "Group.ReadWrite.All", "Directory.Read.All", "User.Read.All", "Files.Read.All", "IdentityProvider.Read.All", "offline_access", };

            var accessToken = new GraphClientConfig().GetToken(new Connection(), scopes).Result;



            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            return request;
        }
    }
}
