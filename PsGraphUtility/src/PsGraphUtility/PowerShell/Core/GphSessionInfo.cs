// src/PsGraphUtility/PowerShell/Core/GphSessionInfo.cs
namespace PsGraphUtility.PowerShell.Core;

public sealed class GphSessionInfo
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string AuthMode { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
}
