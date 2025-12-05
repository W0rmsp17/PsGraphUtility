using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsGraphUtility.Routes.Account;
using PsGraphUtility.Routes.Users;
namespace PsGraphUtility.Routes.Users;

public static class UserRoutes
{
    private const string BaseV1 = "https://graph.microsoft.com/v1.0";

    public static string Users() => $"{BaseV1}/users";
    public static string UserById(string id) => $"{BaseV1}/users/{id}";
    public static string UserMemberOf(string id) => $"{BaseV1}/users/{id}/memberOf";
    public static string MailboxSettings(string id) => $"{BaseV1}/users/{id}/mailboxSettings";

}
