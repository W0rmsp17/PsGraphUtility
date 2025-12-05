using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace PsGraphUtility.Graph.Exchange.Users.Models
{
    public sealed class GraphMailboxUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserPrincipalName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        public string? TimeZone { get; set; }
        public string? Language { get; set; }

        public bool AutomaticRepliesEnabled { get; set; }
        public string? AutoReplyInternalMessage { get; set; }
        public string? AutoReplyExternalMessage { get; set; }

        public string? Signature { get; set; }  
    }
}
