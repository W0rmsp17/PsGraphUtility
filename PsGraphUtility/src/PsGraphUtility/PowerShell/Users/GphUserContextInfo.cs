using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PsGraphUtility.PowerShell.Users;

public sealed class GphUserContextInfo
{
    public string? UserId { get; init; }
    public string? UserPrincipalName { get; init; }
    public string? DisplayName { get; init; }
}
