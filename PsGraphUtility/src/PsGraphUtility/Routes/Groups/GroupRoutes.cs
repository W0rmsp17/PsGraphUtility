using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsGraphUtility.Routes.Groups;

public static class GroupRoutes
{
    private const string BaseV1 = "https://graph.microsoft.com/v1.0";

    public static string Groups() => $"{BaseV1}/groups";

    public static string GroupById(string id) => $"{BaseV1}/groups/{id}";

    // Members of a group
    public static string Members(string id) =>
        $"{BaseV1}/groups/{id}/members";

    // Owners of a group
    public static string Owners(string id) =>
        $"{BaseV1}/groups/{id}/owners";

    public static string Filter(string filter) =>
        $"{BaseV1}/groups?$filter={filter}";
}
