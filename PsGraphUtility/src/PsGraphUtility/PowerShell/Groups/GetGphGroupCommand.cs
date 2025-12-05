using PsGraphUtility.Graph.Groups.Models;
using System.Management.Automation;
using PsGraphUtility.PowerShell.Users;
using PsGraphUtility.Graph.Users;
using PsGraphUtility.Graph.Users.Interface;

namespace PsGraphUtility.PowerShell.Groups;

[Cmdlet(VerbsCommon.Get, "GphGroup")]
[OutputType(typeof(GraphGroupDto))]
public sealed class GetGphGroupCommand : GroupCmdletBase
{
    /*
.SYNOPSIS
Gets Microsoft 365 / Entra ID groups, a specific group, or groups for a user.

.DESCRIPTION
Flexible group retrieval:
- No parameters         → lists groups (optionally filtered by -Type)
- -Group / -Id          → returns a single group
- -User                 → returns groups the specified user belongs to
- -Members / -Owners    → lists members or owners for a specific group

.PARAMETER Group
Group display name or id. If specified, returns only that group.

.PARAMETER Id
Group id (GUID). Overrides -Group when both are supplied.

.PARAMETER User
User principal name (UPN) or id. Returns groups the user is a member of.

.PARAMETER Type
Optional filter: Unified | Security | MailSecurity | Distribution | All.

.PARAMETER Members
When used with -Group or -Id, lists members of that group.

.PARAMETER Owners
When used with -Group or -Id, lists owners of that group.

.EXAMPLE
Get-GphGroup
Lists all groups.

.EXAMPLE
Get-GphGroup -Type Unified
Lists only Microsoft 365 (Unified) groups.

.EXAMPLE
Get-GphGroup -Group "TestGroup"
Returns the group named 'TestGroup'.

.EXAMPLE
Get-GphGroup -User "Cholbing@plutonix.onmicrosoft.com"
Lists all groups that user belongs to.

.EXAMPLE
Get-GphGroup -Group "TestGroup" -Members
Lists members of the 'TestGroup' group.

.EXAMPLE
Get-GphGroup -Id "9c0c385e-e3e3-4eaf-9570-1f1506173ffb" -Owners
Lists owners for the specified group id.
*/
    [Parameter(Mandatory = false, Position = 0)]
    public string? Group { get; set; }

    [Parameter(Mandatory = false)]
    public string? Id { get; set; }

    [Parameter(Mandatory = false)]
    public string? User { get; set; }

    [Parameter(Mandatory = false)]
    public string? Type { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Members { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Owners { get; set; }
    protected override void ProcessRecord()
    {

        if (GlobalServices.GroupService is null)
            throw new InvalidOperationException("DEBUG: GlobalServices.GroupService is null.");

        if (GlobalServices.GroupMemberService is null)
            throw new InvalidOperationException("DEBUG: GlobalServices.GroupMemberService is null.");

        if (GetGroups is null)
            throw new InvalidOperationException("DEBUG: GetGroups property is null.");

        if (GroupMembers is null)
            throw new InvalidOperationException("DEBUG: GroupMembers property is null.");

        var ctx = RequireContext();


        if (!string.IsNullOrWhiteSpace(User))
        {
            var userGroups = UserGroups      
                .GetGroupsForUserAsync(ctx, User)
                .GetAwaiter()
                .GetResult();

            WriteObject(userGroups, enumerateCollection: true);
            return;
        }


        var hasKey = !string.IsNullOrWhiteSpace(Id) || !string.IsNullOrWhiteSpace(Group);
        var key = !string.IsNullOrWhiteSpace(Id) ? Id! : Group ?? string.Empty;

        if ((Members.IsPresent || Owners.IsPresent) && !hasKey)
        {
            throw new InvalidOperationException(
                "You must specify -Id or -Group when using -Members or -Owners.");
        }

        if (Members.IsPresent)
        {
            var members = GroupMembers
                .GetMembersAsync(ctx, key)
                .GetAwaiter()
                .GetResult();

            WriteObject(members, enumerateCollection: true);
            return;
        }

        if (Owners.IsPresent)
        {
            var owners = GroupMembers
                .GetOwnersAsync(ctx, key)
                .GetAwaiter()
                .GetResult();

            WriteObject(owners, enumerateCollection: true);
            return;
        }

        if (hasKey)
        {
            var grp = GetGroups
                .GetGroupAsync(ctx, key, Type)
                .GetAwaiter()
                .GetResult();

            WriteObject(grp);
            return;
        }

        var list = GetGroups
            .ListGroupsAsync(ctx, Type)
            .GetAwaiter()
            .GetResult();

        WriteObject(list, enumerateCollection: true);
    }
}
