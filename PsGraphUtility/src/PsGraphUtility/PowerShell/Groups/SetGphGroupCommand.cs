using System.Management.Automation;
using PsGraphUtility.Graph.Entra.Groups.Models;

namespace PsGraphUtility.PowerShell.Groups
{
    /*
.SYNOPSIS
Updates basic properties on an Entra ID / M365 group.

.DESCRIPTION
Set-GphGroup updates display name, description, mail nickname and visibility
for an existing group. It does NOT change members or owners – use Add-GphGroup
for membership changes.

.PARAMETER Id
Group id (GUID). Use either -Id or -Group.

.PARAMETER Group
Group key (id or display name). Use either -Id or -Group.

.PARAMETER DisplayName
New display name for the group.

.PARAMETER Description
New description for the group.

.PARAMETER MailNickname
New mailNickname (alias) for the group.

.PARAMETER Visibility
Group visibility: Public | Private | HiddenMembership.

.EXAMPLE
Set-GphGroup -Group 'TestGroup' -Description 'Updated via Set-GphGroup'

.EXAMPLE
Set-GphGroup -Id '9c0c385e-e3e3-4eaf-9570-1f1506173ffb' -DisplayName 'New Name'

.EXAMPLE
Get-GphGroup -Group 'TestGroup' | Set-GphGroup -Visibility Private
    */
    [Cmdlet(VerbsCommon.Set, "GphGroup", DefaultParameterSetName = ById)]
    [OutputType(typeof(GraphGroupDto))]
    public class SetGphGroupCommand : GroupCmdletBase
    {
        private const string ById = "ById";
        private const string ByDisplayName = "ByDisplayName";

        [Parameter(
            Mandatory = true,
            Position = 0,
            ParameterSetName = ById,
            ValueFromPipelineByPropertyName = true)]
        public string Id { get; set; } = null!;

        [Parameter(
            Mandatory = true,
            Position = 0,
            ParameterSetName = ByDisplayName,
            ValueFromPipelineByPropertyName = true)]
        public string Group { get; set; } = null!; 

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string? DisplayName { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string? Description { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string? MailNickname { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateSet("Public", "Private", "HiddenMembership")]
        public string? Visibility { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var bp = MyInvocation.BoundParameters;
            var hasUpdate =
                bp.ContainsKey(nameof(DisplayName)) ||
                bp.ContainsKey(nameof(Description)) ||
                bp.ContainsKey(nameof(MailNickname)) ||
                bp.ContainsKey(nameof(Visibility));

            if (!hasUpdate)
            {
                WriteWarning("No updatable properties were specified. Nothing to change.");
                return;
            }

            var context = RequireContext();
            var groupKey = ParameterSetName == ById ? Id : Group;

            var updated = SetGroups
                .UpdateGroupAsync(
                    context,
                    groupKey,
                    displayName: DisplayName,
                    description: Description,
                    mailNickname: MailNickname,
                    visibility: Visibility)
                .GetAwaiter()
                .GetResult();

            WriteObject(updated);
        }
    }
}
