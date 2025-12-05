using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
using PsGraphUtility.Graph.Exchange.Users.Models;
using PsGraphUtility.PowerShell.Users;

namespace PsGraphUtility.PowerShell.Exchange
{
    [Cmdlet(VerbsCommon.Get, "GphMailboxUser")]
    [OutputType(typeof(GraphMailboxUserDto))]
    public sealed class GetGphMailboxUserCommand : UserCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string User { get; set; } = string.Empty; 

        protected override void ProcessRecord()
        {
            var ctx = RequireContext();

            var result = GlobalServices.MailboxUserService
                .GetMailboxUserAsync(ctx, User)
                .GetAwaiter()
                .GetResult();

            WriteObject(result);
        }
    }
}
