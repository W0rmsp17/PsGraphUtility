//path src/PsGraphUtility/Graph/IGraphClient.cs
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;

namespace PsGraphUtility.Graph;

public interface IGraphClient
{
    Task<HttpResponseMessage> SendAsync(
        GraphAuthContext context,
        HttpRequestMessage request,
        CancellationToken cancellationToken = default);
}
