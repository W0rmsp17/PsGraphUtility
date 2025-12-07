//path src/PsGraphUtility/PowerShell/Users/AddGphUserCommand.cs
using System.Management.Automation;
using PsGraphUtility.Graph.Entra.Users.Helpers;
using PsGraphUtility.Graph.Entra.Users.Models;

namespace PsGraphUtility.PowerShell.Users;

[Cmdlet(VerbsCommon.Add, "GphUser")]
[OutputType(typeof(PSObject))]
public sealed class AddGphUserCommand : UserCmdletBase
{
    [Parameter(Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public string UserPrincipalName { get; set; } = string.Empty;

    [Parameter(Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public string DisplayName { get; set; } = string.Empty;

    [Parameter(Mandatory = false)]
    public string? MailNickname { get; set; }

    [Parameter(Mandatory = false)]
    public string? Password { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter GeneratePassword { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter AccountDisabled { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter ForceChangePasswordNextSignIn { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter ForceChangePasswordNextSignInWithMfa { get; set; }

    protected override void ProcessRecord()
    {
        var ctx = RequireContext();

        if (GeneratePassword.IsPresent && !string.IsNullOrWhiteSpace(Password))
            throw new PSArgumentException("Specify either -Password or -GeneratePassword, not both.");

        var effectivePassword = GeneratePassword.IsPresent
            ? UserPasswordGenerator.GeneratePassword()
            : Password;

        if (string.IsNullOrWhiteSpace(effectivePassword))
            throw new PSArgumentException("Password is required unless -GeneratePassword is used.");

        bool forceChange = ForceChangePasswordNextSignIn.IsPresent || !MyInvocation.BoundParameters.ContainsKey(nameof(ForceChangePasswordNextSignIn));

        GraphUserDto user = AddUsers.CreateUserAsync(
                ctx,
                userPrincipalName: UserPrincipalName,
                displayName: DisplayName,
                mailNickname: MailNickname,
                password: effectivePassword,
                accountEnabled: !AccountDisabled.IsPresent,
                forceChangePasswordNextSignIn: forceChange,
                forceChangePasswordNextSignInWithMfa: ForceChangePasswordNextSignInWithMfa.IsPresent)
            .GetAwaiter()
            .GetResult();

        var result = new PSObject();
        result.Properties.Add(new PSNoteProperty("Id", user.Id));
        result.Properties.Add(new PSNoteProperty("UserPrincipalName", user.UserPrincipalName));
        result.Properties.Add(new PSNoteProperty("DisplayName", user.DisplayName));
        result.Properties.Add(new PSNoteProperty("Password", effectivePassword));

        WriteObject(result);
    }
}
