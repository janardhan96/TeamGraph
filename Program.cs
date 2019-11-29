using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamsGraph
{
    class Program
    {
        public static void Main()
        {
            GraphClient graphClient = new GraphClient(new GraphClientConfig());
            var s = graphClient.GetTeams(new Connection() { UserName = "" });
            s.Wait(); 

        }
    }
}
