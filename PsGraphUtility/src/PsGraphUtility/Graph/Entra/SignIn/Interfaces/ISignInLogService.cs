using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Entra.SignIn.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PsGraphUtility.Graph.Entra.SignIn.Interfaces
{
    public interface ISignInLogService
    {
        Task<IReadOnlyList<GraphSignInDto>> GetSignInsAsync(
            GraphAuthContext context,
            int? days = null,          
            string? userKey = null,    
            CancellationToken cancellationToken = default);
    }
}
