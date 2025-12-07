using PsGraphUtility.Auth;
using PsGraphUtility.Graph.HTTP;
using PsGraphUtility.Routes.Groups;
using PsGraphUtility.Graph.Entra.Groups.Interface;
using PsGraphUtility.Graph.Interface;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PsGraphUtility.Graph.Entra.Groups.Helpers
{
    public sealed class GroupLookupService : IGroupLookupService
    {
        private readonly IGraphClient _graph;

        public GroupLookupService(IGraphClient graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        }

        public async Task<string> ResolveGroupIdAsync(
            GraphAuthContext context,
            string idOrNameOrMail,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(idOrNameOrMail))
                throw new ArgumentException("Group key must be provided.", nameof(idOrNameOrMail));

            // If it already looks like a GUID, just use it
            if (Guid.TryParse(idOrNameOrMail, out _))
                return idOrNameOrMail;

            var key = idOrNameOrMail.Trim();

            // displayName eq 'X' OR mail eq 'X' OR mailNickname eq 'X'
            var filter = $"displayName eq '{key.Replace("'", "''")}'" +
                         $" or mail eq '{key.Replace("'", "''")}'" +
                         $" or mailNickname eq '{key.Replace("'", "''")}'";

            var url = GroupRoutes.Groups() +
                      $"?$filter={Uri.EscapeDataString(filter)}" +
                      "&$select=id,displayName,mail,mailNickname&$top=2";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _graph.SendAsync(context, request, cancellationToken)
                                       .ConfigureAwait(false);
           // var body = await response.Content.ReadAsStringAsync(cancellationToken)
           //                                  .ConfigureAwait(false);

            var body = await GraphResponseHelper.EnsureSuccessAsync(
                response,
                "some operation description",
                cancellationToken);

            using var doc = JsonDocument.Parse(body);
            var arr = doc.RootElement.GetProperty("value").EnumerateArray().ToList();

            if (arr.Count == 0)
                throw new GraphAuthException($"No group found matching '{idOrNameOrMail}'.");

            if (arr.Count > 1)
                throw new GraphAuthException($"Multiple groups match '{idOrNameOrMail}'. Please use -Id.");

            return arr[0].GetProperty("id").GetString() ?? string.Empty;
        }
    }
}
