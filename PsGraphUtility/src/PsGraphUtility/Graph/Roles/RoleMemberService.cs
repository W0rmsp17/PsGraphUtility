using System.Text;
using System.Text.Json;

using PsGraphUtility.Auth;
using PsGraphUtility.Graph.HTTP;
using PsGraphUtility.Graph.Roles.Interfaces;
using PsGraphUtility.Graph.Users.Interface;
using PsGraphUtility.Graph.Users.Models;
using PsGraphUtility.Routes.Roles;

namespace PsGraphUtility.Graph.Roles
{
    public sealed class RoleMemberService : IRoleMemberService
    {
        private readonly IGraphClient _graph;
        private readonly IGetRoleService _roles;
        private readonly IUserLookupService _userLookup;

        public RoleMemberService(
            IGraphClient graph,
            IGetRoleService roles,
            IUserLookupService userLookup)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
            _userLookup = userLookup ?? throw new ArgumentNullException(nameof(userLookup));
        }

        private async Task<string> ResolveRoleIdAsync(
            GraphAuthContext context,
            string roleKey,
            CancellationToken cancellationToken)
        {
            var role = await _roles.GetRoleAsync(context, roleKey, cancellationToken)
                                   .ConfigureAwait(false);
            return role.Id;
        }

        public async Task AddMemberAsync(
            GraphAuthContext context,
            string roleKey,
            string userKey,
            CancellationToken cancellationToken = default)
        {
            var roleId = await ResolveRoleIdAsync(context, roleKey, cancellationToken)
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
                RoleRoutes.DirectoryRoleMembers(roleId) + "/$ref")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _graph.SendAsync(context, request, cancellationToken)
                                       .ConfigureAwait(false);

            await GraphResponseHelper.EnsureSuccessAsync(
                response,
                $"add member '{userKey}' to role '{roleKey}'",
                cancellationToken);
        }

        public async Task RemoveMemberAsync(
            GraphAuthContext context,
            string roleKey,
            string userKey,
            CancellationToken cancellationToken = default)
        {
            var roleId = await ResolveRoleIdAsync(context, roleKey, cancellationToken)
                               .ConfigureAwait(false);
            var userId = await _userLookup.ResolveUserIdAsync(context, userKey, cancellationToken)
                                          .ConfigureAwait(false);

            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                RoleRoutes.DirectoryRoleMembers(roleId) + $"/{userId}/$ref");

            var response = await _graph.SendAsync(context, request, cancellationToken)
                                       .ConfigureAwait(false);

            await GraphResponseHelper.EnsureSuccessAsync(
                response,
                $"remove member '{userKey}' from role '{roleKey}'",
                cancellationToken);
        }

        public async Task<IReadOnlyList<GraphUserDto>> GetMembersAsync(
            GraphAuthContext context,
            string roleKey,
            CancellationToken cancellationToken = default)
        {
            var roleId = await ResolveRoleIdAsync(context, roleKey, cancellationToken)
                               .ConfigureAwait(false);

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                RoleRoutes.DirectoryRoleMembers(roleId));

            var response = await _graph.SendAsync(context, request, cancellationToken)
                                       .ConfigureAwait(false);

            var body = await GraphResponseHelper.EnsureSuccessAsync(
                response,
                $"get members for role '{roleKey}'",
                cancellationToken);

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("value")
                .EnumerateArray()
                .Where(IsUserObject)
                .Select(MapUser)
                .ToList();
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
            Mail = root.TryGetProperty("mail", out var mail) ? mail.GetString() : null
        };
    }
}
