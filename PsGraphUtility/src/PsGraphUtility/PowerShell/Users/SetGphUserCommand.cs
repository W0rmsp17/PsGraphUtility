using System.Management.Automation;
using PsGraphUtility.Graph.Users.Models;

namespace PsGraphUtility.PowerShell.Users;

[Cmdlet(VerbsCommon.Set, "GphUser")]
[OutputType(typeof(GraphUserDto))]
public sealed class SetGphUserCommand : UserCmdletBase
{
    [Parameter(Mandatory = true, Position = 0)]
    public string User { get; set; } = string.Empty;

    [Parameter(Mandatory = false)]
    public string? DisplayName { get; set; }

    [Parameter(Mandatory = false)]
    public string? Mail { get; set; }

    [Parameter(Mandatory = false)]
    public string? JobTitle { get; set; }

    [Parameter(Mandatory = false)]
    public string? MobilePhone { get; set; }

    [Parameter(Mandatory = false)]
    public string? OfficeLocation { get; set; }

    [Parameter(Mandatory = false)]
    public string[]? BusinessPhones { get; set; }

    [Parameter(Mandatory = false)]
    public string? Password { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter ForceChangePassword { get; set; }

    /// <summary>
    /// True = block sign-in, False = explicitly unblock..null 
    /// Usage: -BlockSignIn:$true / -BlockSignIn:$false
    /// </summary>
    [Parameter(Mandatory = false)]
    public bool? BlockSignIn { get; set; }

    protected override void ProcessRecord()
    {
        var ctx = RequireContext();

        bool? forceChange = ForceChangePassword.IsPresent ? true : (bool?)null;

        var updated = SetUsers
            .UpdateUserAsync(
                ctx,
                userIdOrUpn: User,
                displayName: DisplayName,
                mail: Mail,
                jobTitle: JobTitle,
                mobilePhone: MobilePhone,
                officeLocation: OfficeLocation,
                businessPhones: BusinessPhones,
                password: Password,
                forceChangePassword: forceChange,
                blockSignIn: BlockSignIn)
            .GetAwaiter()
            .GetResult();

        WriteObject(updated);
    }

}
