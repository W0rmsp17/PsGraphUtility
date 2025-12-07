//Path src/PsGraphUtility/PowerShell/Users/GetGphUserCommand.cs
using System.Management.Automation;
using PsGraphUtility.Graph.Entra.Users.Interface;
using PsGraphUtility.Graph.Entra.Users.Helpers;
using PsGraphUtility.PowerShell.Users;
using PsGraphUtility.Graph.Entra.Users;
using PsGraphUtility.Graph.Entra.Users.Models;

namespace PsGraphUtility.PowerShell.Users;

[Cmdlet(VerbsCommon.Get, "GphUser")]
[OutputType(typeof(GraphUserDto))]
public sealed class GetGphUserCommand : UserCmdletBase
{
    [Parameter(Mandatory = false, Position = 0)]
    public string? User { get; set; }

    protected override void ProcessRecord()
    {
        var ctx = RequireContext();

        if (string.IsNullOrWhiteSpace(User))
        {
            var list = GetUsers
                .ListUsersAsync(ctx)
                .GetAwaiter()
                .GetResult();

            WriteObject(list, enumerateCollection: true);
        }
        else
        {
            var user = GetUsers
                .GetUserAsync(ctx, User)
                .GetAwaiter()
                .GetResult();

            WriteObject(user);
        }
    }
}
