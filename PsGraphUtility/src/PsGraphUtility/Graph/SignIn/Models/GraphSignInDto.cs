using System;
using System.Collections.Generic;

namespace PsGraphUtility.Graph.SignIn.Models
{
    public sealed class GraphSignInDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTimeOffset? CreatedDateTime { get; set; }

        public string? UserDisplayName { get; set; }
        public string? UserPrincipalName { get; set; }

        public string? AppDisplayName { get; set; }
        public string? IpAddress { get; set; }
        public string? ClientAppUsed { get; set; }

        public string? ConditionalAccessStatus { get; set; }
        public bool? IsInteractive { get; set; }
        public string? RiskDetail { get; set; }

        public string? ResourceDisplayName { get; set; }

        // Status
        public int? StatusErrorCode { get; set; }
        public string? StatusFailureReason { get; set; }
        public string? StatusAdditionalDetails { get; set; }

        // Location
        public string? LocationCity { get; set; }
        public string? LocationState { get; set; }
        public string? LocationCountryOrRegion { get; set; }
        public double? LocationLatitude { get; set; }
        public double? LocationLongitude { get; set; }

        // CA policies (flattened list)
        public IReadOnlyList<GraphSignInConditionalAccessPolicyDto> AppliedConditionalAccessPolicies
        { get; set; } = Array.Empty<GraphSignInConditionalAccessPolicyDto>();
    }

    public sealed class GraphSignInConditionalAccessPolicyDto
    {
        public string Id { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Result { get; set; }
    }
}
