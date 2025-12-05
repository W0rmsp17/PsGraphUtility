using PsGraphUtility.Auth;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PsGraphUtility.Graph.HTTP
{
    public static class GraphResponseHelper
    {
        public static async Task<string> EnsureSuccessAsync(
            HttpResponseMessage response,
            string operation,
            CancellationToken cancellationToken = default)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken)
                                             .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return body;

            string? graphMessage = null;
            string? graphCode = null;

            try
            {
                using var doc = JsonDocument.Parse(body);
                var err = doc.RootElement.GetProperty("error");
                graphMessage = err.GetProperty("message").GetString();
                if (err.TryGetProperty("code", out var codeProp))
                    graphCode = codeProp.GetString();
            }
            catch
            {
                
            }

            var msg =
                $"{operation} failed. " +
                $"Status: {(int)response.StatusCode} {response.ReasonPhrase}. " +
                (graphCode is not null ? $"Code: {graphCode}. " : string.Empty) +
                (graphMessage is not null ? $"Message: {graphMessage}" : $"Body: {body}");

            throw new GraphAuthException(msg);
        }
    }
}
