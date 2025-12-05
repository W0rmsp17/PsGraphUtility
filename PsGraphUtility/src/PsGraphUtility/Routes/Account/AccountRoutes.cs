using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsGraphUtility.Routes.Account;
using PsGraphUtility.Routes.Users;
namespace PsGraphUtility.Routes.Account;


public static class AccountRoutes
{
    private const string BaseV1 = "https://graph.microsoft.com/v1.0";
    public static string Applications() => $"{BaseV1}/applications";
    public static string Domains() => $"{BaseV1}/domains";
}
