using System;
using System.Management.Automation;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Exchange.Users.Interface;
using PsGraphUtility.Graph.Exchange.Users.Models;
using PsGraphUtility.PowerShell;

namespace PsGraphUtility.PowerShell.Exchange
{
    /*
.SYNOPSIS
Gets mailbox info for a user, optionally including stats and folder details.

.DESCRIPTION
Wraps multiple Exchange Online cmdlets behind a single surface:
- Get-EXOMailbox
- Get-EXOMailboxStatistics
- Get-EXOMailboxFolderStatistics
- Get-EXOMailboxFolderPermission

By default (no detail switches), returns a summary object.
Use the switches to include additional Exchange data.

.PARAMETER User
User principal name or object id of the mailbox user.

.PARAMETER Mailbox
Include core mailbox properties (Get-EXOMailbox).

.PARAMETER MailboxStats
Include mailbox statistics (Get-EXOMailboxStatistics).

.PARAMETER FolderStats
Include folder statistics (Get-EXOMailboxFolderStatistics).

.PARAMETER FolderPermissions
Include folder permissions (Get-EXOMailboxFolderPermission) for key folders.

.EXAMPLE
Get-GphMailboxUser -User 'alice@contoso.com'
Gets basic mailbox summary.

.EXAMPLE
Get-GphMailboxUser -User 'alice@contoso.com' -Mailbox -MailboxStats
Gets mailbox core properties and statistics.

.EXAMPLE
Get-GphMailboxUser -User 'alice@contoso.com' -FolderStats -FolderPermissions
Gets folder stats and folder permissions only.
*/
    [Cmdlet(VerbsCommon.Get, "GphMailboxUser")]
    [OutputType(typeof(GraphMailboxUserDto))]
    public sealed class GetGphMailboxUserCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string User { get; set; } = string.Empty;

        [Parameter]
        public SwitchParameter Mailbox { get; set; }

        [Parameter]
        public SwitchParameter MailboxStats { get; set; }

        [Parameter]
        public SwitchParameter FolderStats { get; set; }

        [Parameter]
        public SwitchParameter FolderPermissions { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var ctx = GlobalServices.AuthOrchestrator.GetDefaultContext()
                      ?? throw new InvalidOperationException(
                          "No active Graph session. Run Connect-GphTenant first.");

            IGetMailboxUserService svc = GlobalServices.MailboxUserService
                                          ?? throw new InvalidOperationException(
                                              "MailboxUserService is not configured in GlobalServices.");

            var anyDetail =
                Mailbox.IsPresent ||
                MailboxStats.IsPresent ||
                FolderStats.IsPresent ||
                FolderPermissions.IsPresent;

            var includeMailbox = anyDetail ? Mailbox.IsPresent : true;
            var includeMailboxStats = anyDetail ? MailboxStats.IsPresent : false;
            var includeFolderStats = anyDetail ? FolderStats.IsPresent : false;
            var includeFolderPermissions = anyDetail ? FolderPermissions.IsPresent : false;

            var result = svc
                .GetMailboxUserAsync(
                    ctx,
                    User,
                    includeMailbox,
                    includeMailboxStats,
                    includeFolderStats,
                    includeFolderPermissions)
                .GetAwaiter()
                .GetResult();

            WriteObject(result);
        }
    }
}
