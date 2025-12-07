//path src/PsGraphUtility/PowerShell/GlobalServices.cs

using Microsoft.Identity.Client;
using PsGraphUtility.Auth;
using PsGraphUtility.Bootstrap;
using PsGraphUtility.Core;
using PsGraphUtility.Graph;
using PsGraphUtility.Graph.Entra.Devices;
using PsGraphUtility.Graph.Exchange.Users.Interface;
using PsGraphUtility.Graph.Entra.Groups;
using PsGraphUtility.Graph.Entra.Groups.Helpers;
using PsGraphUtility.Graph.Entra.Roles;
using PsGraphUtility.Graph.Entra.SignIn;
using PsGraphUtility.Graph.Entra.Users;
using PsGraphUtility.Graph.Entra;
using PsGraphUtility.Graph.Exchange.Users;
using PsGraphUtility.Graph.Entra.Devices.Interfaces;
using PsGraphUtility.Graph.Entra.Groups.Interface;
using PsGraphUtility.Graph.Entra.Roles.Interfaces;
using PsGraphUtility.Graph.Entra.SignIn.Interfaces;
using PsGraphUtility.Graph.Entra.Users.Interface;
using PsGraphUtility.Graph.Interface;
using PsGraphUtility.PowerShell.Exchange;
using PsGraphUtility.Graph.Entra.Users.Helpers;



namespace PsGraphUtility.PowerShell
{
    public static class GlobalServices
    {
        private static IGraphTokenProvider? _tokenProvider;
        private static IAuthOrchestrator? _auth;
        private static IGraphClient? _graph;
        private static IBootstrapService? _bootstrap;

        // USERS
        private static IUserLookupService? _userLookup;
        private static IGetUserService? _getUsers;
        private static ISetUserService? _setUsers;
        private static IAddUserService? _addUsers;
        private static IUserGroupService? _userGroupService;


        // GROUPS
        private static IGroupLookupService? _groupLookup;
        private static IGetGroupService? _getGroups;
   
        private static ISetGroupService? _setGroups;
        private static IAddGroupService? _addGroups;
        private static IGroupMemberService? _groupMemberService;
        // Other contextual state
        private static UserContext? _userContext;
        private static ISignInLogService? _signInLogService;


        private static IGetDeviceService? _deviceService;
        private static ISetDeviceService? _setDeviceService;

        private static IGetRoleService? _roleService;
        private static IRoleMemberService? _roleMemberService;
        private static ISetRoleService? _setRoleService;
        private static IGetMailboxUserService? _mailboxUsers;

        // Public service accessors

        public static UserContext UserContext
            => _userContext ??= new UserContext();

        public static IGraphTokenProvider TokenProvider
            => _tokenProvider ??= BuildTokenProvider();

        public static IAuthOrchestrator AuthOrchestrator
            => _auth ??= new AuthOrchestrator(TokenProvider);

        public static IGraphClient GraphClient
            => _graph ??= new GraphClient(TokenProvider);

        public static IBootstrapService BootstrapService
            => _bootstrap ??= new BootstrapService(GraphClient);

        //USERS
        public static IUserGroupService UserGroupService
            => _userGroupService ??= new UserGroupService(GraphClient, UserLookup);
        public static IUserLookupService UserLookup
            => _userLookup ??= new UserLookupService(GraphClient);

        public static IGetUserService GetUserService
            => _getUsers ??= new GetUserService(GraphClient, UserLookup);

        public static ISetUserService SetUserService
            => _setUsers ??= new SetUserService(GraphClient, UserLookup);

        public static IAddUserService AddUserService
            => _addUsers ??= new AddUserService(GraphClient);
        
        /// 
        /// GROUPS 
        /// 

        public static IGroupLookupService GroupLookup
            => _groupLookup ??= new GroupLookupService(GraphClient);

        public static IGetGroupService GroupService
            => _getGroups ??= new GetGroupService(GraphClient, GroupLookup);

        public static IGroupMemberService GroupMemberService
            => _groupMemberService ??= new GroupMemberService(GraphClient, GroupLookup, UserLookup);


        public static IGetGroupService? GetGroupAsync { get; internal set; }

        public static ISetGroupService SetGroupService
            => _setGroups ??= new SetGroupService(GraphClient, GroupLookup, GroupService);

        public static IAddGroupService AddGroupService
            => _addGroups ??= new AddGroupService(GraphClient, GroupService);

        // SignIn
        public static ISignInLogService SignInLogService
            => _signInLogService ??= new SignInLogService(GraphClient, UserLookup);


        //Device
        public static IGetDeviceService DeviceService
        => _deviceService ??= new GetDeviceService(GraphClient);

        public static ISetDeviceService SetDeviceService
            => _setDeviceService ??= new SetDeviceService(GraphClient, DeviceService);

        // Role
        public static IGetRoleService RoleService
            => _roleService ??= new GetRoleService(GraphClient, UserLookup);

        public static IRoleMemberService RoleMemberService
            => _roleMemberService ??= new RoleMemberService(GraphClient, RoleService, UserLookup);

        public static ISetRoleService SetRoleService
            => _setRoleService ??= new SetRoleService(RoleService, RoleMemberService);
        // Exchange
        public static IGetMailboxUserService MailboxUserService
            => _mailboxUsers ??= new GetMailboxUserService(GraphClient, UserLookup);


        //*
        // Internal builder
        // 
        private static IGraphTokenProvider BuildTokenProvider()
        {
            var publicClientId = "9d9bf72a-5d1a-4445-b80f-b3c1abc950a1";

            return new MsalGraphTokenProvider(
                publicClientId,
                deviceCode =>
                {
                    Console.WriteLine(deviceCode.Message);
                    return Task.CompletedTask;
                });
        }
    }
}
