// src/PsGraphUtility/Bootstrap/BootstrapGraphClient.cs
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Bootstrap.Interface;

namespace PsGraphUtility.Bootstrap;

public sealed class BootstrapGraphClient : IBootstrapGraphClient
{
    private readonly HttpClient _http;

    public BootstrapGraphClient(HttpClient? http = null)
    {
        _http = http ?? new HttpClient();
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(
        GraphAuthContext context,
        HttpMethod method,
        string path,
        CancellationToken ct)
    {
       
        var request = new HttpRequestMessage(method, $"https://graph.microsoft.com/v1.0/{path}");

       
        return request;
    }

    public Task<(string appId, string objectId, string displayName)> CreateBootstrapAppAsync(
        GraphAuthContext context,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<(string upn, string objectId)> CreateServiceAccountAsync(
        GraphAuthContext context,
        string? desiredUpn,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
