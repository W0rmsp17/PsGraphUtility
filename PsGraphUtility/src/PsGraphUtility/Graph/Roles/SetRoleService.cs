using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Roles.Interfaces;
using PsGraphUtility.Graph.Roles.Models;
using PsGraphUtility.Graph.Users.Models;

namespace PsGraphUtility.Graph.Roles
{
    public sealed class SetRoleService : ISetRoleService
    {
        private readonly IGetRoleService _roles;
        private readonly IRoleMemberService _roleMembers;

        public SetRoleService(IGetRoleService roles, IRoleMemberService roleMembers)
        {
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
            _roleMembers = roleMembers ?? throw new ArgumentNullException(nameof(roleMembers));
        }

        public async Task<GraphRoleDto> SyncRoleMembersAsync(
            GraphAuthContext context,
            string roleKey,
            IEnumerable<string> members,
            bool ensureExact,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(roleKey))
                throw new ArgumentException("Role key must be provided.", nameof(roleKey));

            var desired = members
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(m => m.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (desired.Length == 0)
                throw new GraphAuthException("No usable members provided for role sync.");

            IReadOnlyList<GraphUserDto> current = await _roleMembers
                .GetMembersAsync(context, roleKey, cancellationToken)
                .ConfigureAwait(false);

            var currentKeys = current
                .Select(u => string.IsNullOrWhiteSpace(u.UserPrincipalName) ? u.Id : u.UserPrincipalName)
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var desiredSet = new HashSet<string>(desired, StringComparer.OrdinalIgnoreCase);

            var toAdd = desiredSet.Except(currentKeys).ToList();
            var toRemove = ensureExact ? currentKeys.Except(desiredSet).ToList() : new List<string>();

            foreach (var u in toAdd)
                await _roleMembers.AddMemberAsync(context, roleKey, u, cancellationToken).ConfigureAwait(false);

            foreach (var u in toRemove)
                await _roleMembers.RemoveMemberAsync(context, roleKey, u, cancellationToken).ConfigureAwait(false);

            return await _roles.GetRoleAsync(context, roleKey, cancellationToken).ConfigureAwait(false);
        }
    }
}
