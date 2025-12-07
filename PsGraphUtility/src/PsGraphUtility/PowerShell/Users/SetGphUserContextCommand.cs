// path src/PsGraphUtility/PowerShell/Users/SetGphUserContextCommand.cs
using System.Management.Automation;
using PsGraphUtility.Graph.Entra.Users;
using PsGraphUtility.Graph.Entra.Users.Models;

namespace PsGraphUtility.PowerShell.Users;

[Cmdlet(VerbsCommon.Set, "GphUserContext")]
[OutputType(typeof(GphUserContextInfo))]
public sealed class SetGphUserContextCommand : UserCmdletBase
{
    [Parameter(Mandatory = true, Position = 0)]
    public string User { get; set; } = string.Empty;

    protected override void ProcessRecord()
    {
        var ctx = RequireContext();

        GraphUserDto user = GetUsers
            .GetUserAsync(ctx, User)
            .GetAwaiter()
            .GetResult();

        var uc = GlobalServices.UserContext;
        uc.UserId = user.Id;
        uc.UserPrincipalName = user.UserPrincipalName;
        uc.DisplayName = user.DisplayName;

        WriteObject(new GphUserContextInfo
        {
            UserId = user.Id,
            UserPrincipalName = user.UserPrincipalName,
            DisplayName = user.DisplayName
        });
    }
}
