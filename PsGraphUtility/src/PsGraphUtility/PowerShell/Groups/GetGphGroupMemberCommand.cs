using System.Management.Automation;
using PsGraphUtility.Graph.Entra.Users.Models;

namespace PsGraphUtility.PowerShell.Groups
{
    [Cmdlet(VerbsCommon.Get, "GphGroupMember", DefaultParameterSetName = ByGroup)]
    [OutputType(typeof(GraphUserDto))]
    public class GetGphGroupMemberCommand : GroupCmdletBase
    {
        private const string ByGroup = "ByGroup";

        [Parameter(
            Mandatory = true,
            Position = 0,
            ParameterSetName = ByGroup,
            ValueFromPipelineByPropertyName = true)]
        public string Group { get; set; } = null!;

        [Parameter]
        public SwitchParameter Members { get; set; }

        [Parameter]
        public SwitchParameter Owners { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var ctx = RequireContext();
            var gm = GroupMembers;

            var wantMembers = Members.IsPresent || !Owners.IsPresent;
            var wantOwners = Owners.IsPresent;

            if (wantMembers)
            {
                var members = gm.GetMembersAsync(ctx, Group).GetAwaiter().GetResult();
                foreach (var m in members)
                    WriteObject(m);
            }

            if (wantOwners)
            {
                var owners = gm.GetOwnersAsync(ctx, Group).GetAwaiter().GetResult();
                foreach (var o in owners)
                    WriteObject(o);
            }
        }
    }
}
