using System;
using System.Linq;
using System.Management.Automation;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Entra.Users.Models;
using System.Threading;
using PsGraphUtility.Graph.Entra.Roles.Models;

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
