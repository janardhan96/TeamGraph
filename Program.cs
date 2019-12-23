using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
    }
}
