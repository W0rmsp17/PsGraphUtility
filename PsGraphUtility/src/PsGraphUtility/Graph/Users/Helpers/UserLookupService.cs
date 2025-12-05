// src/PsGraphUtility/Graph/Users/UserLookupService.cs
using PsGraphUtility.Auth;
using PsGraphUtility.Graph;
using PsGraphUtility.Graph.HTTP;
using PsGraphUtility.Graph.Users.Interface;
using PsGraphUtility.Routes.Users;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PsGraphUtility.Graph.Users.Helpers;

public sealed class UserLookupService : IUserLookupService
{
    private readonly IGraphClient _graph;

    public UserLookupService(IGraphClient graph) => _graph = graph;

    public async Task<string> ResolveUserIdAsync(
        GraphAuthContext context,
        string upnOrId,
        CancellationToken cancellationToken = default)
    {

        if (Guid.TryParse(upnOrId, out _))
            return upnOrId;

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            UserRoutes.UserById(upnOrId));   // /users/{upn}

        var response = await _graph.SendAsync(context, request, cancellationToken);
  //      var body = await response.Content.ReadAsStringAsync(cancellationToken);

        var body = await GraphResponseHelper.EnsureSuccessAsync(
            response,
            "some operation description",
            cancellationToken);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        return root.GetProperty("id").GetString() ?? string.Empty;
    }
}
