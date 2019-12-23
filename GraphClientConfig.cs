using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace TeamsGraph
{
    public class GraphClientConfig
    {
        private Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult ADAccessTokenInfo { get; set; }
        private Microsoft.Identity.Client.AuthenticationResult AccessTokenInfo { get; set; }
        public IPublicClientApplication PublicClientApp { get; private set; }
        public AuthenticationContext AuthenticationContext { get; private set; }
        public IConfidentialClientApplication ConfidentialClientApp { get; private set; }
        public GraphClientConfig()
        {
            PublicClientApp = PublicClientApplicationBuilder.Create("2bae50d2-90aa-4b11-aca4-3caf162934f9")
                .Build();

            string cacheFolder = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"..\..\..\..");
            string adalV3cacheFileName = Path.Combine(cacheFolder, "cacheAdalV3.bin");
            string msalCacheFileName = Path.Combine(cacheFolder, "cacheMsal.bin");

            var token = new TokenCacheHelper(PublicClientApp.UserTokenCache, msalCacheFileName);

            FilesBasedTokenCache tokenCache = new FilesBasedTokenCache(adalV3cacheFileName, msalCacheFileName);

            AuthenticationContext = new AuthenticationContext("https://login.microsoftonline.com/common", tokenCache);
            //AuthenticationContext = new AuthenticationContext("https://login.microsoftonline.com/2c2f8827-b525-4182-ad82-775151c73a85", tokenCache);

        }

        /// <summary>
        /// Get Token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public async Task<string> GetTokenForUserAsync(Connection connection, IEnumerable<string> scopes)
        {
            if (AccessTokenInfo?.AccessToken == null || AccessTokenInfo.ExpiresOn.Subtract(DateTimeOffset.UtcNow) > TimeSpan.Zero)
            {
                IEnumerable<IAccount> account = await PublicClientApp.GetAccountsAsync();
                var requriedscopes = new List<string>();
                try
                {
                    AccessTokenInfo = await PublicClientApp.AcquireTokenSilent(scopes, account.FirstOrDefault(s => s.Username == "navateja")).ExecuteAsync();
                }
                //To posiblities 
                // 1- Accounts Dosn't Exist
                // 2- Microsoft.Identity.Client.MsalUiRequiredException
                catch (Exception)
                {
                    //await GetTokenByRefereshToken(scopes, "93ded07f-9eff-40f0-91b9-eb61022eeced.8a92f134-8bf1-47b2-9e5e-7c1653e22f85-login.windows.net-refreshtoken-2bae50d2-90aa-4b11-aca4-3caf162934f9--");

                    AccessTokenInfo = await PublicClientApp.AcquireTokenInteractive(scopes).WithAuthority("https://login.microsoftonline.com/7a096f3f-68c6-450a-9281-1e29226ebda9").WithPrompt(Microsoft.Identity.Client.Prompt.Consent).ExecuteAsync();

                }
            }
            return AccessTokenInfo.AccessToken;
        }

        public async Task<string> GetToken(Connection connection, IEnumerable<string> scopes)
        {
            IEnumerable<IAccount> account = await PublicClientApp.GetAccountsAsync();
            var requriedscopes = new List<string>();
            try
            {
                string redirectUri = "urn:ietf:wg:oauth:2.0:oob";// "urn://teamsgraph";// ; https://graph.microsoft.com
                ADAccessTokenInfo = await AuthenticationContext.AcquireTokenAsync("https://management.azure.com", "829d83fb-a70b-4d61-8939-ed1e016da9f0", new Uri(redirectUri), new PlatformParameters(PromptBehavior.SelectAccount));
                ADAccessTokenInfo = await AuthenticationContext.AcquireTokenSilentAsync("https://rohanpvtlimited.sharepoint.com", "2bae50d2-90aa-4b11-aca4-3caf162934f9");
                ADAccessTokenInfo = await AuthenticationContext.AcquireTokenSilentAsync("https://spmig.sharepoint.com", "829d83fb-a70b-4d61-8939-ed1e016da9f0");

                var AccessTokenInfo = await PublicClientApp.AcquireTokenInteractive(scopes).WithPrompt(Prompt.Consent).ExecuteAsync();
                var appRt = PublicClientApp as IByRefreshToken;
                var result = appRt.AcquireTokenByRefreshToken(scopes, "AQABAAAAAACQN9QBRU3jT6bcBQLZNUj7UT3OmtlJb-jtX3aLB2awIN_bcmYpBZcImfJgeq80Mv9vtwZk53Je0efqAN1hO8Dneo4RjWqTcwaiqcyXR1ZLpMh2bIc7mPJOKTtulLG1-iCXZTvdcunNIvbsLYAi5SSzMf3xZ_JsfA4bwXkR-HCRlOkZZW-K12g36MMQDKNW4ktFs5qHxY6FMq2Q6CBD_uRSpz26yTgtVds_aLMNM6FVu2ZUFZUlxyBoAtzOrEEG_Cm94o9xhfgt6WMTwmp-_PD5FS-elP23XIBnoEy_w0L9DnHCfwuoT6qrio_hN3fMbzLNHOSxATGfyE9nf0OCXyqNW03gNFxJphnzoE4x6L4xZ0o_ErzH1LmSdaHfjrfYFumEoKr0EiQMvWtCIgBM6qHFiZ3aTrYj6cm_F_KmOxGWheQEKLjds56lvawtxtCacmkO5knVylXKcC8RsUjwEVjLpMWfKFXOWDcHJjTvWKNl7_HEVzGhVNfj7pP8mIaxyghcgduTH47K3iK2nhmy5mhUzgT5MhvJlqeptxLhFWw5LfZWTFkh_qp3mmmvvM8lvrV5KmlyKrGVsAqctXUzeF5WK9nycYqtMQb-jv9V8QewmyEZEliExlHloMsBePHIDy0uZK4bqABecq__cMqY_e8Z36H77biaFqwqT2U3aAQPc5eD3vcuG-o7itP738RF5mfSsaCsmWdsYeuOUVVrjhP3_L_Ve-_c3aZodFRreMQAi3MmrISkGytzG7xDZAFpJ5bCjecKv6fLR5A2Xeoje0oi-aGxiVNLI_Cq98Rt_dE03fVp2OJFhdQumEh5OJ20IBdU0tQHj02bzZwMw__A4UYCu2jN9DcFcSJTdWzdA_H0owJdmaMIccb4vGYDbSFLaN9VLkSI6FcYA_SFxp1Gh0yGgULPpJzORziyRaS5OTi-bSAA").ExecuteAsync().ConfigureAwait(false);
                var s = result.GetAwaiter().GetResult();

                return ADAccessTokenInfo.AccessToken;
            }
            //To posiblities 
            // 1- Accounts Dosn't Exist
            // 2- Microsoft.Identity.Client.MsalUiRequiredException
            catch (Exception ex)
            {
                //await GetTokenByRefereshToken(scopes, "93ded07f-9eff-40f0-91b9-eb61022eeced.8a92f134-8bf1-47b2-9e5e-7c1653e22f85-login.windows.net-refreshtoken-2bae50d2-90aa-4b11-aca4-3caf162934f9--");

                //ADAccessTokenInfo = await AuthenticationContext.AcquireTokenAsync("https://spmig.sharepoint.com", "2bae50d2-90aa-4b11-aca4-3caf162934f9", new Uri(redirectUri), new PlatformParameters(PromptBehavior.SelectAccount));
                string redirectUri = "urn:ietf:wg:oauth:2.0:oob";
                ADAccessTokenInfo = await AuthenticationContext.AcquireTokenAsync("https://rohanpvtlimited.sharepoint.com", "2bae50d2-90aa-4b11-aca4-3caf162934f9", new Uri(redirectUri), new PlatformParameters(PromptBehavior.SelectAccount));
                return ADAccessTokenInfo.AccessToken;
            }
        }

        public async Task<string> GetTokenByRefereshToken(IEnumerable<string> scopes, string refreshToken)
        {
            IByRefreshToken token = PublicClientApp as IByRefreshToken;
            AccessTokenInfo = await token.AcquireTokenByRefreshToken(null, refreshToken).ExecuteAsync();
            return null;
        }
    }
}
