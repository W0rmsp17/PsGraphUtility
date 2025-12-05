//Path src/PsGraphUtility/PowerShell/Users/GetGphUserCommand.cs
using System.Management.Automation;
using PsGraphUtility.Graph.Users.Models;
using PsGraphUtility.Graph.Users.Interface;
using PsGraphUtility.Graph.Users.Helpers;
using PsGraphUtility.PowerShell.Users;
using PsGraphUtility.Graph.Users;

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
