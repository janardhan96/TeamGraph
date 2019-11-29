using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace TeamsGraph
{
    public class GraphClientConfig
    {
        private AuthenticationResult AccessTokenInfo { get; set; }
        public IPublicClientApplication IdentityClientApp { get; private set; }
        public GraphClientConfig()
        {
            IdentityClientApp = PublicClientApplicationBuilder.Create("2bae50d2-90aa-4b11-aca4-3caf162934f9").Build();
            TokenCacheHelper.EnableSerialization(IdentityClientApp.UserTokenCache);
        }

        /// <summary>
        /// Get Token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public async Task<string> GetTokenForUserAsync(Connection connection, IEnumerable<string> scopes)
        {
            if (AccessTokenInfo?.AccessToken == null || AccessTokenInfo.ExpiresOn.Subtract(DateTimeOffset.UtcNow) > TimeSpan.Zero)
            {
                IEnumerable<IAccount> account = await IdentityClientApp.GetAccountsAsync();
                var requriedscopes = new List<string>();
                try
                {
                    AccessTokenInfo = await IdentityClientApp.AcquireTokenSilent(scopes, account.FirstOrDefault()).ExecuteAsync();
                }
                //To posiblities 
                // 1- Accounts Dosn't Exist
                // 2- Microsoft.Identity.Client.MsalUiRequiredException
                catch (Exception)
                {
                    //await GetTokenByRefereshToken(scopes, "93ded07f-9eff-40f0-91b9-eb61022eeced.8a92f134-8bf1-47b2-9e5e-7c1653e22f85-login.windows.net-refreshtoken-2bae50d2-90aa-4b11-aca4-3caf162934f9--");
                    AccessTokenInfo = await IdentityClientApp.AcquireTokenInteractive(scopes).ExecuteAsync();
                }
            }
            return AccessTokenInfo.AccessToken;
        }

        public async Task<string> GetTokenByRefereshToken(IEnumerable<string> scopes,string refreshToken)
        {
            IByRefreshToken token = IdentityClientApp as IByRefreshToken;
            AccessTokenInfo = await token.AcquireTokenByRefreshToken(null, refreshToken).ExecuteAsync();
            return null;
        }
    }
}
