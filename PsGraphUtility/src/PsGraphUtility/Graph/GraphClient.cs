// src/PsGraphUtility/Graph/GraphClient.cs
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Interface;


namespace PsGraphUtility.Graph;

public sealed class GraphClient : IGraphClient
{
    private readonly HttpClient _http;
    private readonly IGraphTokenProvider _tokenProvider;

    public GraphClient(IGraphTokenProvider tokenProvider, HttpClient? http = null)
    {
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        _http = http ?? new HttpClient();
    }

    public async Task<HttpResponseMessage> SendAsync(
        GraphAuthContext context,
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        var token = await _tokenProvider.AcquireTokenAsync(context, cancellationToken)
                                       .ConfigureAwait(false);

        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);

        return await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
