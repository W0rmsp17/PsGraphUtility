// src/PsGraphUtility/Core/UserContext.cs
namespace PsGraphUtility.Core;

public sealed class UserContext
{
    public string? UserId { get; set; }
    public string? UserPrincipalName { get; set; }
    public string? DisplayName { get; set; }

    public bool HasUser =>
        !string.IsNullOrWhiteSpace(UserId) ||
        !string.IsNullOrWhiteSpace(UserPrincipalName);

    public void Clear()
    {
        UserId = null;
        UserPrincipalName = null;
        DisplayName = null;
    }
}
