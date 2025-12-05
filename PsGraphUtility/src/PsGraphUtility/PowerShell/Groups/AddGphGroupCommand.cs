using System;
using System.Linq;
using System.Management.Automation;
using PsGraphUtility.Graph.Groups.Models;

namespace PsGraphUtility.PowerShell.Groups
{
    /*
.SYNOPSIS
Creates a new group or updates membership on an existing group.

.DESCRIPTION
Add-GphGroup has two modes:
- Create: creates a new M365 / security group (with optional members/owners).
- Members: adds/removes members or owners on an existing group.

.PARAMETER DisplayName
Display name for the new group (Create set only).

.PARAMETER MailNickname
Mail nickname (alias) for the new group (Create set only).

.PARAMETER Description
Optional description for the new group.

.PARAMETER Visibility
Group visibility: Public | Private | HiddenMembership (Create set only).

.PARAMETER GroupType
Group type: Unified (M365) or Security (default: Unified).

.PARAMETER Members
Initial members to add when creating a group.

.PARAMETER Owners
Initial owners to add when creating a group.

.PARAMETER Group
Existing group key (id or display name) for membership updates (Members set).

.PARAMETER AddMember
Users to add as members to an existing group.

.PARAMETER RemoveMember
Users to remove as members from an existing group.

.PARAMETER AddOwner
Users to add as owners to an existing group.

.PARAMETER RemoveOwner
Users to remove as owners from an existing group.

.EXAMPLE
Add-GphGroup -DisplayName 'PsGraph-TestGroup' -MailNickname 'PsGraphTest' `
  -Description 'Test group' -Visibility Public `
  -Members 'user1@contoso.com','user2@contoso.com' `
  -Owners  'admin@contoso.com'

.EXAMPLE
Add-GphGroup -Group 'PsGraph-TestGroup' -AddMember 'user3@contoso.com'

.EXAMPLE
Add-GphGroup -Group 'PsGraph-TestGroup' -AddOwner 'admin2@contoso.com'
    */
    [Cmdlet(VerbsCommon.Add, "GphGroup", DefaultParameterSetName = Create)]
    [OutputType(typeof(GraphGroupDto))]
    public class AddGphGroupCommand : GroupCmdletBase
    {
        private const string Create = "Create";
        private const string MembershipSet = "Members";

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = Create)]
        public string DisplayName { get; set; } = null!;

        [Parameter(Mandatory = true, Position = 1, ParameterSetName = Create)]
        public string MailNickname { get; set; } = null!;

        [Parameter(ParameterSetName = Create)]
        public string? Description { get; set; }

        [Parameter(ParameterSetName = Create)]
        [ValidateSet("Public", "Private", "HiddenMembership")]
        public string? Visibility { get; set; }

        [Parameter(ParameterSetName = Create)]
        [ValidateSet("Unified", "Security")]
        public string GroupType { get; set; } = "Unified";

        [Parameter(ParameterSetName = Create)]
        public string[]? Members { get; set; }

        [Parameter(ParameterSetName = Create)]
        public string[]? Owners { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = MembershipSet)]
        public string Group { get; set; } = null!; 

        [Parameter(ParameterSetName = MembershipSet)]
        public string[]? AddMember { get; set; }

        [Parameter(ParameterSetName = MembershipSet)]
        public string[]? RemoveMember { get; set; }

        [Parameter(ParameterSetName = MembershipSet)]
        public string[]? AddOwner { get; set; }

        [Parameter(ParameterSetName = MembershipSet)]
        public string[]? RemoveOwner { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var ctx = RequireContext();
            var gm = GroupMembers;

            if (ParameterSetName == Create)
            {
                var group = AddGroups.CreateGroupAsync(
                        ctx,
                        DisplayName,
                        MailNickname,
                        Description,
                        Visibility,
                        GroupType)
                    .GetAwaiter()
                    .GetResult();

                var gKey = group.Id;

                var members = Members?
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .Select(m => m.Trim())
                    .ToArray();

                if (members is { Length: > 0 })
                    foreach (var u in members)
                        gm.AddMemberAsync(ctx, gKey, u).GetAwaiter().GetResult();

                var owners = Owners?
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Select(o => o.Trim())
                    .ToArray();

                if (owners is { Length: > 0 })
                    foreach (var u in owners)
                        gm.AddOwnerAsync(ctx, gKey, u).GetAwaiter().GetResult();

                WriteObject(group);
                return;
            }

            var groupKey = Group;

            var addMembers = AddMember?
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(m => m.Trim())
                .ToArray();

            if (addMembers is { Length: > 0 })
                foreach (var u in addMembers)
                    gm.AddMemberAsync(ctx, groupKey, u).GetAwaiter().GetResult();

            var removeMembers = RemoveMember?
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(m => m.Trim())
                .ToArray();

            if (removeMembers is { Length: > 0 })
                foreach (var u in removeMembers)
                    gm.RemoveMemberAsync(ctx, groupKey, u).GetAwaiter().GetResult();

            var addOwners = AddOwner?
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select(o => o.Trim())
                .ToArray();

            if (addOwners is { Length: > 0 })
                foreach (var u in addOwners)
                    gm.AddOwnerAsync(ctx, groupKey, u).GetAwaiter().GetResult();

            var removeOwners = RemoveOwner?
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select(o => o.Trim())
                .ToArray();

            if (removeOwners is { Length: > 0 })
                foreach (var u in removeOwners)
                    gm.RemoveOwnerAsync(ctx, groupKey, u).GetAwaiter().GetResult();
        }
    }
}
