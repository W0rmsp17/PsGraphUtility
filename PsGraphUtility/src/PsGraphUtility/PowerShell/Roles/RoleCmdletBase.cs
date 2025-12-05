using System;
using System.Management.Automation;
using System;

using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Roles.Interfaces;

namespace PsGraphUtility.PowerShell.Roles
{
    public abstract class RoleCmdletBase : PSCmdlet
    {
        protected GraphAuthContext RequireContext()
        {
            var ctx = GlobalServices.AuthOrchestrator.GetDefaultContext();
            if (ctx is null)
                throw new InvalidOperationException(
                    "No active Graph session. Run Connect-GphTenant first.");
            return ctx;
        }

        protected IGetRoleService Roles => GlobalServices.RoleService;
        protected IRoleMemberService RoleMembers => GlobalServices.RoleMemberService;
        protected ISetRoleService SetRoles => GlobalServices.SetRoleService;
    }
}
