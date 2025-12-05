using System.Management.Automation;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Users.Interface;

namespace PsGraphUtility.PowerShell.Users;

public abstract class UserCmdletBase : PSCmdlet
{
    protected GraphAuthContext RequireContext()
    {
        var ctx = GlobalServices.AuthOrchestrator.GetDefaultContext();
        if (ctx is null)
            throw new PSInvalidOperationException("No Graph session. Run Connect-GphTenant first.");
        return ctx;
    }

    protected IGetUserService GetUsers => GlobalServices.GetUserService;
    protected ISetUserService SetUsers => GlobalServices.SetUserService;
    protected IAddUserService AddUsers => GlobalServices.AddUserService;
}
