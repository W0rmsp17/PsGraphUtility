namespace PsGraphUtility.Bootstrap;

public sealed class BootstrapProfile
{
    public string TenantId { get; set; } = string.Empty;

    public string? AppId { get; init; }
    public string? AppObjectId { get; init; }
    public string? AppDisplayName { get; init; }

    public string? ServiceAccountUpn { get; init; }
    public string? ServiceAccountObjectId { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAtUtc { get; init; }

    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(AppId) &&
        !string.IsNullOrWhiteSpace(ServiceAccountUpn);
}
