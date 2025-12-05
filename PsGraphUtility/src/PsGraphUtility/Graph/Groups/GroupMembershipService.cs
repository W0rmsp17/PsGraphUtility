using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph;
using PsGraphUtility.Graph.Groups.Interface;
using PsGraphUtility.Graph.HTTP;
using PsGraphUtility.Graph.Users.Interface;
using PsGraphUtility.Graph.Users.Models;
using PsGraphUtility.Routes.Groups;

namespace PsGraphUtility.Graph.Groups
{
    public sealed class GroupMemberService : IGroupMemberService
    {
        private readonly IGraphClient _graph;
        private readonly IGroupLookupService _groupLookup;
        private readonly IUserLookupService _userLookup;

        public GroupMemberService(
            IGraphClient graph,
            IGroupLookupService groupLookup,
            IUserLookupService userLookup)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _groupLookup = groupLookup ?? throw new ArgumentNullException(nameof(groupLookup));
            _userLookup = userLookup ?? throw new ArgumentNullException(nameof(userLookup));
        }

        public async Task<IReadOnlyList<GraphUserDto>> GetMembersAsync(
            GraphAuthContext context,
            string groupIdOrName,
            CancellationToken cancellationToken = default)
        {
            var groupId = await _groupLookup.ResolveGroupIdAsync(
                context, groupIdOrName, cancellationToken).ConfigureAwait(false);

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                GroupRoutes.Members(groupId));

            var response = await _graph.SendAsync(context, request, cancellationToken)
                                       .ConfigureAwait(false);

            var body = await GraphResponseHelper.EnsureSuccessAsync(
                response,
                $"get group members for '{groupIdOrName}'",
                cancellationToken);

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("value")
                                  .EnumerateArray()
                                  .Where(IsUserObject)
                                  .Select(MapUser)
                                  .ToList();
        }

        public async Task<IReadOnlyList<GraphUserDto>> GetOwnersAsync(
            GraphAuthContext context,
            string groupIdOrName,
            CancellationToken cancellationToken = default)
        {
            var groupId = await _groupLookup.ResolveGroupIdAsync(
                context, groupIdOrName, cancellationToken).ConfigureAwait(false);

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                GroupRoutes.Owners(groupId));

            var response = await _graph.SendAsync(context, request, cancellationToken)
                                       .ConfigureAwait(false);

            var body = await GraphResponseHelper.EnsureSuccessAsync(
                response,
                $"get group owners for '{groupIdOrName}'",
                cancellationToken);

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("value")
                                  .EnumerateArray()
                                  .Where(IsUserObject)
                                  .Select(MapUser)
                                  .ToList();
        }

        public async Task AddMemberAsync(
            GraphAuthContext context,
            string groupKey,
            string userKey,
            CancellationToken cancellationToken = default)
        {
            var groupId = await _groupLookup.ResolveGroupIdAsync(context, groupKey, cancellationToken)
                                            .ConfigureAwait(false);
            var userId = await _userLookup.ResolveUserIdAsync(context, userKey, cancellationToken)
                                          .ConfigureAwait(false);

            var body = new Dictionary<string, string>
            {
                ["@odata.id"] = $"https://graph.microsoft.com/v1.0/directoryObjects/{userId}"
            };

            var json = JsonSerializer.Serialize(body);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                GroupRoutes.Members(groupId) + "/$ref")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _graph.SendAsync(context, request, cancellationToken)
                                       .ConfigureAwait(false);

            await GraphResponseHelper.EnsureSuccessAsync(
                response,
                $"add member '{userKey}' to group '{groupKey}'",
                cancellationToken);
        }

        public async Task RemoveMemberAsync(
            GraphAuthContext context,
            string groupKey,
            string userKey,
            CancellationToken cancellationToken = default)
        {
            var groupId = await _groupLookup.ResolveGroupIdAsync(context, groupKey, cancellationToken)
                                            .ConfigureAwait(false);
            var userId = await _userLookup.ResolveUserIdAsync(context, userKey, cancellationToken)
                                          .ConfigureAwait(false);

            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                GroupRoutes.Members(groupId) + $"/{userId}/$ref");

            var response = await _graph.SendAsync(context, request, cancellationToken)
                                       .ConfigureAwait(false);

            await GraphResponseHelper.EnsureSuccessAsync(
                response,
                $"remove member '{userKey}' from group '{groupKey}'",
                cancellationToken);
        }

        public async Task AddOwnerAsync(
            GraphAuthContext context,
            string groupKey,
            string userKey,
            CancellationToken cancellationToken = default)
        {
            var groupId = await _groupLookup.ResolveGroupIdAsync(context, groupKey, cancellationToken)
                                            .ConfigureAwait(false);
            var userId = await _userLookup.ResolveUserIdAsync(context, userKey, cancellationToken)
                                          .ConfigureAwait(false);

            var body = new Dictionary<string, string>
            {
                ["@odata.id"] = $"https://graph.microsoft.com/v1.0/directoryObjects/{userId}"
            };

            var json = JsonSerializer.Serialize(body);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                GroupRoutes.Owners(groupId) + "/$ref")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _graph.SendAsync(context, request, cancellationToken)
                                       .ConfigureAwait(false);

            await GraphResponseHelper.EnsureSuccessAsync(
                response,
                $"add owner '{userKey}' to group '{groupKey}'",
                cancellationToken);
        }

        public async Task RemoveOwnerAsync(
            GraphAuthContext context,
            string groupKey,
            string userKey,
            CancellationToken cancellationToken = default)
        {
            var groupId = await _groupLookup.ResolveGroupIdAsync(context, groupKey, cancellationToken)
                                            .ConfigureAwait(false);
            var userId = await _userLookup.ResolveUserIdAsync(context, userKey, cancellationToken)
                                          .ConfigureAwait(false);

            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                GroupRoutes.Owners(groupId) + $"/{userId}/$ref");

            var response = await _graph.SendAsync(context, request, cancellationToken)
                                       .ConfigureAwait(false);

            await GraphResponseHelper.EnsureSuccessAsync(
                response,
                $"remove owner '{userKey}' from group '{groupKey}'",
                cancellationToken);
        }

        private static bool IsUserObject(JsonElement e) =>
            e.TryGetProperty("@odata.type", out var t) &&
            t.GetString() == "#microsoft.graph.user";

        private static GraphUserDto MapUser(JsonElement root) => new()
        {
            Id = root.GetProperty("id").GetString() ?? string.Empty,
            UserPrincipalName = root.TryGetProperty("userPrincipalName", out var upn)
                ? upn.GetString() ?? string.Empty
                : string.Empty,
            DisplayName = root.TryGetProperty("displayName", out var dn)
                ? dn.GetString() ?? string.Empty
                : string.Empty,
            Mail = root.TryGetProperty("mail", out var mail) ? mail.GetString() : null,
            JobTitle = root.TryGetProperty("jobTitle", out var jt) ? jt.GetString() : null,
            MobilePhone = root.TryGetProperty("mobilePhone", out var mp) ? mp.GetString() : null,
            OfficeLocation = root.TryGetProperty("officeLocation", out var ol) ? ol.GetString() : null,
            BusinessPhones = root.TryGetProperty("businessPhones", out var bp) &&
                             bp.ValueKind == JsonValueKind.Array
                ? bp.EnumerateArray().Select(p => p.GetString() ?? string.Empty).ToArray()
                : Array.Empty<string>()
        };
    }
}
