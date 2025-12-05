using System;
using System.Management.Automation;
using PsGraphUtility.Graph.Roles.Models;

namespace PsGraphUtility.PowerShell.Roles
{
    /*
    .SYNOPSIS
    Gets Entra ID directory roles, a specific role, or roles for a user.

    .DESCRIPTION
    Flexible role retrieval:
    - No parameters      → lists all directory roles
    - -Role / -Id        → returns a single role
    - -User              → returns directory roles assigned to a user

    .PARAMETER Role
    Role display name or id. If specified (or -Id is provided), returns only that role.

    .PARAMETER Id
    Directory role id (GUID). Overrides -Role when both are supplied.

    .PARAMETER User
    User principal name (UPN) or object id.
    When supplied, returns all directory roles assigned to that user.
    Cannot be combined with -Role or -Id.

    .EXAMPLE
    Get-GphRole
    Lists all directory roles in the tenant.

    .EXAMPLE
    Get-GphRole -Role "Global Administrator"
    Gets the 'Global Administrator' role definition.

    .EXAMPLE
    Get-GphRole -Id "62e90394-69f5-4237-9190-012177145e10"
    Gets the role for the specified role id.

    .EXAMPLE
    Get-GphRole -User "alice@contoso.com"
    Lists all directory roles assigned to alice@contoso.com.
    */

    [Cmdlet(VerbsCommon.Get, "GphRole")]
    [OutputType(typeof(GraphRoleDto))]
    public sealed class GetGphRoleCommand : RoleCmdletBase
    {
        [Parameter(Mandatory = false, Position = 0)]
        public string? Role { get; set; }

        [Parameter(Mandatory = false)]
        public string? Id { get; set; }

        [Parameter(Mandatory = false)]
        public string? User { get; set; }

        protected override void ProcessRecord()
        {
            var ctx = RequireContext();

            var hasKey = !string.IsNullOrWhiteSpace(Id) || !string.IsNullOrWhiteSpace(Role);
            var key = !string.IsNullOrWhiteSpace(Id) ? Id! : Role ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(User))
            {
                if (hasKey)
                    throw new InvalidOperationException(
                        "Specify either -User or -Role/-Id, not both.");

                var userRoles = Roles
                    .GetRolesForUserAsync(ctx, User)
                    .GetAwaiter()
                    .GetResult();

                WriteObject(userRoles, enumerateCollection: true);
                return;
            }

            if (hasKey)
            {
                var role = Roles
                    .GetRoleAsync(ctx, key)
                    .GetAwaiter()
                    .GetResult();

                WriteObject(role);
                return;
            }

            var list = Roles
                .ListRolesAsync(ctx)
                .GetAwaiter()
                .GetResult();

            WriteObject(list, enumerateCollection: true);
        }
    }
}
