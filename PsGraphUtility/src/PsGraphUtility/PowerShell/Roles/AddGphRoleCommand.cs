using System.Management.Automation;
using PsGraphUtility.Graph.Roles.Models;

namespace PsGraphUtility.PowerShell.Roles
{
    [Cmdlet(VerbsCommon.Add, "GphRole")]
    [OutputType(typeof(GraphRoleDto))]
    public sealed class AddGphRoleCommand : RoleCmdletBase
    {
        // Role id or displayName
        [Parameter(Mandatory = true, Position = 0)]
        public string Role { get; set; } = null!;

        // Users to add to the role
        [Parameter(Mandatory = false)]
        public string[]? AddMember { get; set; }

        // Users to remove from the role
        [Parameter(Mandatory = false)]
        public string[]? RemoveMember { get; set; }

        protected override void ProcessRecord()
        {
            var ctx = RequireContext();

            if ((AddMember is null || AddMember.Length == 0) &&
                (RemoveMember is null || RemoveMember.Length == 0))
            {
                throw new PSArgumentException(
                    "You must specify -AddMember and/or -RemoveMember.");
            }

            // Add members
            if (AddMember is { Length: > 0 })
            {
                foreach (var u in AddMember)
                {
                    RoleMembers.AddMemberAsync(ctx, Role, u)
                               .GetAwaiter()
                               .GetResult();
                }
            }

            // Remove members
            if (RemoveMember is { Length: > 0 })
            {
                foreach (var u in RemoveMember)
                {
                    RoleMembers.RemoveMemberAsync(ctx, Role, u)
                               .GetAwaiter()
                               .GetResult();
                }
            }

            // Return the role object after modification
            var roleDto = Roles
                .GetRoleAsync(ctx, Role)
                .GetAwaiter()
                .GetResult();

            WriteObject(roleDto);
        }
    }
}
