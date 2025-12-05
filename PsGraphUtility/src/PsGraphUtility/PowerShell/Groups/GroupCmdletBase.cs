using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Groups.Interface;
using PsGraphUtility.Graph.Users.Interface;
using System;
using System.Management.Automation;

namespace PsGraphUtility.PowerShell.Groups;

public abstract class GroupCmdletBase : PSCmdlet
{
    protected GraphAuthContext RequireContext()
    {
        var ctx = GlobalServices.AuthOrchestrator.GetDefaultContext();
        if (ctx is null)
            throw new InvalidOperationException(
                "No active Graph session. Run Connect-GphTenant first.");
        return ctx;
    }

    protected ISetGroupService SetGroups => GlobalServices.SetGroupService;
    protected IUserGroupService UserGroups => GlobalServices.UserGroupService;
    public IGetGroupService GetGroups => GlobalServices.GroupService;

    protected IAddGroupService AddGroups => GlobalServices.AddGroupService;

    protected IGroupMemberService GroupMembers => GlobalServices.GroupMemberService;
}
