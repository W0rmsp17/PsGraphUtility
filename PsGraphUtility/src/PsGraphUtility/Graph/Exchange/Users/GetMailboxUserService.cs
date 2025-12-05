using PsGraphUtility.Auth;
using PsGraphUtility.Graph;
using PsGraphUtility.Graph.Exchange.Users.Interface;
using PsGraphUtility.Graph.Exchange.Users.Models;
using PsGraphUtility.Graph.HTTP;
using PsGraphUtility.Graph.Users.Interface;
using PsGraphUtility.Graph.Users.Models;
using PsGraphUtility.Routes.Users;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PsGraphUtility.Graph.Exchange.Users
{
    public sealed class GetMailboxUserService : IGetMailboxUserService
    {
        private readonly IGraphClient _graph;
        private readonly IUserLookupService _lookup;

        public GetMailboxUserService(IGraphClient graph, IUserLookupService lookup)
        {
            _graph = graph;
            _lookup = lookup;
        }

        public async Task<GraphMailboxUserDto> GetMailboxUserAsync(
            GraphAuthContext context,
            string userIdOrUpn,
            CancellationToken cancellationToken = default)
        {
            var userId = await _lookup.ResolveUserIdAsync(
                context, userIdOrUpn, cancellationToken).ConfigureAwait(false);

            using var userReq = new HttpRequestMessage(
                HttpMethod.Get,
                UserRoutes.UserById(userId));

            var userRes = await _graph.SendAsync(context, userReq, cancellationToken)
                                      .ConfigureAwait(false);

            var userBody = await GraphResponseHelper.EnsureSuccessAsync(
                userRes,
                $"get user '{userIdOrUpn}'",
                cancellationToken);

            using var userDoc = JsonDocument.Parse(userBody);
            var uRoot = userDoc.RootElement;

            var dto = new GraphMailboxUserDto
            {
                Id = uRoot.GetProperty("id").GetString() ?? string.Empty,
                UserPrincipalName = uRoot.GetProperty("userPrincipalName").GetString() ?? string.Empty,
                DisplayName = uRoot.GetProperty("displayName").GetString() ?? string.Empty
            };

            using var mbxReq = new HttpRequestMessage(
                HttpMethod.Get,
                UserRoutes.MailboxSettings(userId));

            var mbxRes = await _graph.SendAsync(context, mbxReq, cancellationToken)
                                     .ConfigureAwait(false);

            var mbxBody = await GraphResponseHelper.EnsureSuccessAsync(
                mbxRes,
                $"get mailboxSettings for '{userIdOrUpn}'",
                cancellationToken);

            using var mbxDoc = JsonDocument.Parse(mbxBody);
            var mRoot = mbxDoc.RootElement;

            if (mRoot.TryGetProperty("timeZone", out var tz))
                dto.TimeZone = tz.GetString();

            if (mRoot.TryGetProperty("language", out var lang) &&
                lang.ValueKind == JsonValueKind.Object &&
                lang.TryGetProperty("displayName", out var ldisp))
                dto.Language = ldisp.GetString();

            if (mRoot.TryGetProperty("automaticRepliesSetting", out var ar) &&
                ar.ValueKind == JsonValueKind.Object)
            {
                var status = ar.TryGetProperty("status", out var st) ? st.GetString() : null;
                dto.AutomaticRepliesEnabled =
                    string.Equals(status, "alwaysEnabled", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(status, "scheduled", System.StringComparison.OrdinalIgnoreCase);

                if (ar.TryGetProperty("internalReplyMessage", out var irm))
                    dto.AutoReplyInternalMessage = irm.GetString();

                if (ar.TryGetProperty("externalReplyMessage", out var erm))
                    dto.AutoReplyExternalMessage = erm.GetString();
            }


            return dto;
        }
    }
}
