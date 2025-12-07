using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.HTTP;
using PsGraphUtility.Routes.Roles;
using PsGraphUtility.Graph.Entra.Roles.Interfaces;
using PsGraphUtility.Graph.Entra.Roles.Models;
using PsGraphUtility.Graph.Entra.Users.Interface;
using PsGraphUtility.Graph.Interface;

namespace PsGraphUtility.Graph.Entra.Roles
{
    public sealed class GetRoleService : IGetRoleService
    {
        private readonly IGraphClient _graph;
        private readonly IUserLookupService _userLookup;

        public GetRoleService(IGraphClient graph, IUserLookupService userLookup)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _userLookup = userLookup ?? throw new ArgumentNullException(nameof(userLookup));
        }

        public async Task<GraphRoleDto> GetRoleAsync(
            GraphAuthContext context,
            string roleKey,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(roleKey))
                throw new ArgumentException("Role key must be provided.", nameof(roleKey));


            if (Guid.TryParse(roleKey, out _))
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, RoleRoutes.DirectoryRole(roleKey));
                var res = await _graph.SendAsync(context, req, cancellationToken).ConfigureAwait(false);
                var body = await GraphResponseHelper.EnsureSuccessAsync(res, $"get role '{roleKey}'", cancellationToken);
                using var doc = JsonDocument.Parse(body);
                return MapRole(doc.RootElement);
            }
            var url = $"{RoleRoutes.DirectoryRoles()}?$filter=displayName eq '{roleKey.Replace("'", "''")}'";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _graph.SendAsync(context, request, cancellationToken).ConfigureAwait(false);
            var body2 = await GraphResponseHelper.EnsureSuccessAsync(response, $"get role '{roleKey}'", cancellationToken);

            using var doc2 = JsonDocument.Parse(body2);
            var match = doc2.RootElement.GetProperty("value").EnumerateArray().FirstOrDefault();
            if (match.ValueKind == JsonValueKind.Undefined)
                throw new GraphAuthException($"Role '{roleKey}' not found.");

            return MapRole(match);
        }

        public async Task<IReadOnlyList<GraphRoleDto>> ListRolesAsync(
            GraphAuthContext context,
            CancellationToken cancellationToken = default)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, RoleRoutes.DirectoryRoles());
            var res = await _graph.SendAsync(context, req, cancellationToken).ConfigureAwait(false);
            var body = await GraphResponseHelper.EnsureSuccessAsync(res, "list roles", cancellationToken);

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("value")
                     .EnumerateArray()
                     .Select(MapRole)
                     .ToList();
        }

        public async Task<IReadOnlyList<GraphRoleDto>> GetRolesForUserAsync(
            GraphAuthContext context,
            string userKey,
            CancellationToken cancellationToken = default)
        {
            var userId = await _userLookup.ResolveUserIdAsync(context, userKey, cancellationToken)
                                          .ConfigureAwait(false);

            var url = $"https://graph.microsoft.com/v1.0/users/{userId}/memberOf";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            var res = await _graph.SendAsync(context, req, cancellationToken).ConfigureAwait(false);
            var body = await GraphResponseHelper.EnsureSuccessAsync(res, $"get roles for user '{userKey}'", cancellationToken);

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("value")
                     .EnumerateArray()
                     .Where(e => e.GetProperty("@odata.type").GetString() == "#microsoft.graph.directoryRole")
                     .Select(MapRole)
                     .ToList();
        }

        public static GraphRoleDto MapRole(JsonElement e) => new()
        {
            Id = e.GetProperty("id").GetString() ?? string.Empty,
            DisplayName = e.TryGetProperty("displayName", out var dn) ? dn.GetString() : null,
            Description = e.TryGetProperty("description", out var desc) ? desc.GetString() : null,
            RoleTemplateId = e.TryGetProperty("roleTemplateId", out var rt) ? rt.GetString() : null
        };
    }
}
