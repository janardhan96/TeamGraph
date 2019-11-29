using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
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
            GetClientContext(accessToken);
            var graphClient = CreateGraphClient(accessToken);
            var currentUser = await graphClient.Me.Request().GetAsync();

            return null;
        }
        public async Task<string> CreateTeam(Connection connection)
        {
            string[] scopes = { "Group.ReadWrite.All" };
            var accessToken = await GraphClientConfig.GetTokenForUserAsync(connection, scopes);
            var graphClient = CreateGraphClient(accessToken);

            var team = new Team
            {
                MemberSettings = new TeamMemberSettings
                {
                    AllowCreateUpdateChannels = true
                },
                MessagingSettings = new TeamMessagingSettings
                {
                    AllowUserEditMessages = true,
                    AllowUserDeleteMessages = true
                },
                FunSettings = new TeamFunSettings
                {
                    AllowGiphy = true,
                    GiphyContentRating = GiphyRatingType.Strict
                }
            };
            return null;
        }

        public async Task<string> CreateGroup(Connection connection)
        {
            string[] scopes = { "Group.ReadWrite.All" };
            var accessToken = await GraphClientConfig.GetTokenForUserAsync(connection, scopes);
            var graphClient = CreateGraphClient(accessToken);
            return null;
        }
        public void GetClientContext(string accessToken)
        {
            using (ClientContext context = TokenHelper.GetClientContextWithAccessToken("https://spmig.sharepoint.com/sites/abcd", accessToken))
            {
                Web web = context.Web;
                context.Load(web);
                context.ExecuteQuery();
                Console.WriteLine(web.Title);
            }

        }
    }
}
