using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
namespace TeamsGraph
{
    public class TenantGroup
    {
        public string Description { get; set; }
        public string DisplayName { get; set; }
        IEnumerable<string> GroupTypes { get; set; }
        string Mail { get; set; }
        bool? MailEnabled { get; set; }
        string MailNickname { get; set; }
        string Visibility { get; set; }
        bool? SecurityEnabled { get; set; }
    }
}
