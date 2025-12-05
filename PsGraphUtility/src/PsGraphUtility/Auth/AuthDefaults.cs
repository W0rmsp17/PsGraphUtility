namespace PsGraphUtility.Auth;

public static class AuthDefaults
{
    public const string PublicCloudLoginHost = "https://login.microsoftonline.com";

    public static string BuildAuthority(string? tenantId)
    {
        var tenantSegment = string.IsNullOrWhiteSpace(tenantId)
            ? "organizations"
            : tenantId.Trim();

        return $"{PublicCloudLoginHost}/{tenantSegment}/";
    }


    public static readonly string[] BootstrapAdminScopes =
    {
        "Directory.ReadWrite.All",
        "User.ReadWrite.All",
        "Application.ReadWrite.All",
        "offline_access"
    };

    public static readonly string[] UserDelegatedBaseScopes =
    {
    "User.Read",
    "offline_access",
    "Directory.ReadWrite.All",
    "User.ReadWrite.All",
    "Application.ReadWrite.All",
    "Domain.Read.All",
    "AuditLog.Read.All",
    "User.ManageIdentities.All", 
    "User.EnableDisableAccount.All", 
    "User.ReadWrite.All", 
    "Directory.ReadWrite.All",
    "Directory.AccessAsUser.All"
};

}
