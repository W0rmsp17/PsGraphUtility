using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsGraphUtility.Routes.Roles
{
    public static class RoleRoutes
    {
        private const string BaseV1 = "https://graph.microsoft.com/v1.0";

        public static string DirectoryRoles() =>
            $"{BaseV1}/directoryRoles";

        public static string DirectoryRole(string id) =>
            $"{BaseV1}/directoryRoles/{id}";

        public static string DirectoryRoleMembers(string id) =>
            $"{BaseV1}/directoryRoles/{id}/members";
    }
}
