using PsGraphUtility.Auth;
using PsGraphUtility.Graph;
using PsGraphUtility.Graph.Groups;
using PsGraphUtility.Graph.Groups.Models;
using PsGraphUtility.Graph.HTTP;
using PsGraphUtility.Graph.Users.Interface;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PsGraphUtility.Graph.Users
{
    public sealed class UserGroupService : IUserGroupService
    {
        private readonly IGraphClient _graph;
        private readonly IUserLookupService _userLookup;
        private const string BaseV1 = "https://graph.microsoft.com/v1.0";

        public UserGroupService(IGraphClient graph, IUserLookupService userLookup)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _userLookup = userLookup ?? throw new ArgumentNullException(nameof(userLookup));
        }

        public async Task<IReadOnlyList<GraphGroupDto>> GetGroupsForUserAsync(
     GraphAuthContext context,
     string userKey,
     CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userKey))
                throw new ArgumentException("User key must be provided.", nameof(userKey));

            var userId = await _userLookup
                .ResolveUserIdAsync(context, userKey, cancellationToken)
                .ConfigureAwait(false);

            var url = $"{BaseV1}/users/{userId}/memberOf";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _graph
                .SendAsync(context, request, cancellationToken)
                .ConfigureAwait(false);

            var body = await GraphResponseHelper.EnsureSuccessAsync(
                response,
                $"get groups for user '{userKey}'",
                cancellationToken);

            using var doc = JsonDocument.Parse(body);
            var list = new List<GraphGroupDto>();

            foreach (var item in doc.RootElement.GetProperty("value").EnumerateArray())
            {
                if (!item.TryGetProperty("@odata.type", out var t) ||
                    !string.Equals(t.GetString(), "#microsoft.graph.group",
                                   StringComparison.OrdinalIgnoreCase))
                    continue;

                list.Add(MapGroup(item));   // 👈 local mapper below
            }

            return list;
        }

        private static GraphGroupDto MapGroup(JsonElement root) => new()
        {
            Id = root.GetProperty("id").GetString() ?? string.Empty,
            DisplayName = root.TryGetProperty("displayName", out var dn) ? dn.GetString() : null,
            Description = root.TryGetProperty("description", out var desc) ? desc.GetString() : null,
            Mail = root.TryGetProperty("mail", out var mail) ? mail.GetString() : null,
            MailNickname = root.TryGetProperty("mailNickname", out var mn) ? mn.GetString() : null,
            MailEnabled = root.TryGetProperty("mailEnabled", out var me) && me.GetBoolean(),
            SecurityEnabled = root.TryGetProperty("securityEnabled", out var se) && se.GetBoolean(),
            Visibility = root.TryGetProperty("visibility", out var vis) ? vis.GetString() : null,
            GroupTypes = root.TryGetProperty("groupTypes", out var gt) && gt.ValueKind == JsonValueKind.Array
                              ? gt.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray()
                              : Array.Empty<string>()
        };

    }
}
