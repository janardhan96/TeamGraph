using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
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
            string[] scopes = { "Group.ReadWrite.All", "Directory.ReadWrite.All", "Directory.AccessAsUser.All" };
            var accessToken = await GraphClientConfig.GetTokenForUserAsync(connection, scopes);
            var graphClient = CreateGraphClient(accessToken);
            //var groups = await graphClient.Groups.Request().GetAsync();
            var group = new Group
            {
                DisplayName = "Graph",
                Description = "Created using Graph API",
                //Mail = "Graph@spmig.onmicrosoft.com",
                MailEnabled = true,
                MailNickname = "Graph",
                GroupTypes = new List<string> { "Unified"},
                Visibility ="private",
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
            var accessToken = await GraphClientConfig.GetTokenForUserAsync(connection, scopes);
            var graphClient = CreateGraphClient(accessToken);
            var groups = await graphClient.Groups.Request().GetAsync();
            return null;
        }
        //"1a2db6b4-1b5e-4832-9c5b-575703b9b566"

        public async Task<string> GetTeams(Connection connection)
        {
            string[] scopes = { "User.Read.All", "User.ReadWrite.All", "Directory.ReadWrite.All", "Directory.AccessAsUser.All" };
            var accessToken = await GraphClientConfig.GetTokenForUserAsync(connection, scopes);
            var graphClient = CreateGraphClient(accessToken);
            var teams = await graphClient.Teams.Request().GetAsync();
            return null;
        }
    }
}
