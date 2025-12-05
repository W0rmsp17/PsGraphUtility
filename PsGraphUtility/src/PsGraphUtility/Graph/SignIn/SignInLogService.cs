using PsGraphUtility.Auth;
using PsGraphUtility.Graph;
using PsGraphUtility.Graph.HTTP;
using PsGraphUtility.Graph.SignIn.Interface;
using PsGraphUtility.Graph.Users.Interface;
using PsGraphUtility.Graph.SignIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PsGraphUtility.Graph.SignIn
{
    public sealed class SignInLogService : ISignInLogService
    {
        private const string BaseV1 = "https://graph.microsoft.com/v1.0";

        private readonly IGraphClient _graph;
        private readonly IUserLookupService _userLookup;

        public SignInLogService(IGraphClient graph, IUserLookupService userLookup)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _userLookup = userLookup ?? throw new ArgumentNullException(nameof(userLookup));
        }

        public async Task<IReadOnlyList<GraphSignInDto>> GetSignInsAsync(
            GraphAuthContext context,
            int? days = null,
            string? userKey = null,
            CancellationToken cancellationToken = default)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (days is < 0 or > 30)
                throw new ArgumentOutOfRangeException(nameof(days), "days must be between 0 and 30.");

            var filters = new List<string>();

            if (days is int d)
            {
                var from = DateTimeOffset.UtcNow.AddDays(-d);
                var iso = from.ToString("o");
                filters.Add($"createdDateTime ge {iso}");
            }

            if (!string.IsNullOrWhiteSpace(userKey))
            {
                var userId = await _userLookup.ResolveUserIdAsync(context, userKey, cancellationToken)
                                              .ConfigureAwait(false);
                filters.Add($"userId eq '{userId}'");
            }

            var url = $"{BaseV1}/auditLogs/signIns";
            if (filters.Any())
            {
                var filterExpr = string.Join(" and ", filters);
                url += $"?$filter={Uri.EscapeDataString(filterExpr)}";
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _graph.SendAsync(context, request, cancellationToken)
                                       .ConfigureAwait(false);

            var body = await GraphResponseHelper.EnsureSuccessAsync(
                response,
                "list sign-ins",
                cancellationToken);

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("value")
                      .EnumerateArray()
                      .Select(MapSignIn)
                      .ToList();
        }

        private static GraphSignInDto MapSignIn(JsonElement e)
        {
            var dto = new GraphSignInDto
            {
                Id = e.GetProperty("id").GetString() ?? string.Empty,
                CreatedDateTime = e.TryGetProperty("createdDateTime", out var cd) && cd.ValueKind == JsonValueKind.String
                    ? cd.GetDateTimeOffset()
                    : (DateTimeOffset?)null,

                UserDisplayName = e.TryGetProperty("userDisplayName", out var udn) ? udn.GetString() : null,
                UserPrincipalName = e.TryGetProperty("userPrincipalName", out var upn) ? upn.GetString() : null,

                AppDisplayName = e.TryGetProperty("appDisplayName", out var app) ? app.GetString() : null,
                IpAddress = e.TryGetProperty("ipAddress", out var ip) ? ip.GetString() : null,
                ClientAppUsed = e.TryGetProperty("clientAppUsed", out var cau) ? cau.GetString() : null,

                ConditionalAccessStatus = e.TryGetProperty("conditionalAccessStatus", out var cas) ? cas.GetString() : null,
                IsInteractive = e.TryGetProperty("isInteractive", out var inter) && inter.ValueKind != JsonValueKind.Null
                    ? inter.GetBoolean()
                    : (bool?)null,
                RiskDetail = e.TryGetProperty("riskDetail", out var rd) ? rd.GetString() : null,

                ResourceDisplayName = e.TryGetProperty("resourceDisplayName", out var rdn) ? rdn.GetString() : null
            };

            if (e.TryGetProperty("status", out var st) && st.ValueKind == JsonValueKind.Object)
            {
                dto.StatusErrorCode = st.TryGetProperty("errorCode", out var ec) && ec.ValueKind != JsonValueKind.Null
                    ? ec.GetInt32()
                    : (int?)null;
                dto.StatusFailureReason = st.TryGetProperty("failureReason", out var fr) ? fr.GetString() : null;
                dto.StatusAdditionalDetails = st.TryGetProperty("additionalDetails", out var ad) ? ad.GetString() : null;
            }

            if (e.TryGetProperty("location", out var loc) && loc.ValueKind == JsonValueKind.Object)
            {
                dto.LocationCity = loc.TryGetProperty("city", out var c) ? c.GetString() : null;
                dto.LocationState = loc.TryGetProperty("state", out var s) ? s.GetString() : null;
                dto.LocationCountryOrRegion = loc.TryGetProperty("countryOrRegion", out var cr) ? cr.GetString() : null;

                if (loc.TryGetProperty("geoCoordinates", out var geo) && geo.ValueKind == JsonValueKind.Object)
                {
                    dto.LocationLatitude = geo.TryGetProperty("latitude", out var lat) && lat.ValueKind != JsonValueKind.Null
                        ? lat.GetDouble()
                        : (double?)null;
                    dto.LocationLongitude = geo.TryGetProperty("longitude", out var lon) && lon.ValueKind != JsonValueKind.Null
                        ? lon.GetDouble()
                        : (double?)null;
                }
            }

            if (e.TryGetProperty("appliedConditionalAccessPolicies", out var caps) &&
                caps.ValueKind == JsonValueKind.Array)
            {
                var list = new List<GraphSignInConditionalAccessPolicyDto>();
                foreach (var p in caps.EnumerateArray())
                {
                    list.Add(new GraphSignInConditionalAccessPolicyDto
                    {
                        Id = p.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
                        DisplayName = p.TryGetProperty("displayName", out var dn) ? dn.GetString() : null,
                        Result = p.TryGetProperty("result", out var res) ? res.GetString() : null
                    });
                }

                dto.AppliedConditionalAccessPolicies = list;
            }

            return dto;
        }
    }
}
