using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsGraphUtility.Graph.Groups.Models;

public sealed class GraphGroupDto
{
    // Core
    public string Id { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }

    public string? Mail { get; init; }
    public string? MailNickname { get; init; }
    public bool MailEnabled { get; init; }
    public bool SecurityEnabled { get; init; }
    public string[] GroupTypes { get; init; } = System.Array.Empty<string>();

    // Extra – for | Select-Object
    public string? Classification { get; init; }

    public System.DateTimeOffset? CreatedDateTime { get; init; }
    public System.DateTimeOffset? ExpirationDateTime { get; init; }
    public System.DateTimeOffset? RenewedDateTime { get; init; }

    public bool? IsAssignableToRole { get; init; }

    public string? MembershipRule { get; init; }
    public string? MembershipRuleProcessingState { get; init; }

    public System.DateTimeOffset? OnPremisesLastSyncDateTime { get; init; }
    public string? OnPremisesSecurityIdentifier { get; init; }
    public bool? OnPremisesSyncEnabled { get; init; }

    public string? PreferredDataLocation { get; init; }
    public string? PreferredLanguage { get; init; }

    public string[] ProxyAddresses { get; init; } = System.Array.Empty<string>();
    public string[] ResourceBehaviorOptions { get; init; } = System.Array.Empty<string>();
    public string[] ResourceProvisioningOptions { get; init; } = System.Array.Empty<string>();

    public string? Theme { get; init; }
    public string? Visibility { get; init; }
}

