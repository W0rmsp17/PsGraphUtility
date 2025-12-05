using System;
using System.Linq;
using System.Management.Automation;
using PsGraphUtility.Graph.Roles.Models;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Users.Models;
using System.Threading;

namespace PsGraphUtility.PowerShell.Roles
{
    [Cmdlet(VerbsCommon.Set, "GphRole")]
    [OutputType(typeof(GraphRoleDto))]
    public sealed class SetGphRoleCommand : RoleCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Role { get; set; } = null!;

        [Parameter(Mandatory = true)]
        public string[] Members { get; set; } = Array.Empty<string>();

        [Parameter]
        public SwitchParameter EnsureExact { get; set; }

        protected override void ProcessRecord()
        {
            var ctx = RequireContext();

            var cleaned = Members
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(m => m.Trim())
                .ToArray();

            if (cleaned.Length == 0)
            {
                WriteWarning("No usable members provided after trimming; nothing to do.");
                return;
            }

            var role = SetRoles
                .SyncRoleMembersAsync(ctx, Role, cleaned, EnsureExact.IsPresent, CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            WriteObject(role);
        }
    }
}
