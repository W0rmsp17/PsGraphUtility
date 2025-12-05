// src/PsGraphUtility/Graph/Users/Interface/IAddUserService.cs
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Users.Models;

namespace PsGraphUtility.Graph.Users.Interface;

public interface IAddUserService
{
    Task<GraphUserDto> CreateUserAsync(
        GraphAuthContext context,
        string userPrincipalName,
        string displayName,
        string? mailNickname = null,
        string? password = null,
        bool accountEnabled = true,
        bool forceChangePasswordNextSignIn = true,
        bool forceChangePasswordNextSignInWithMfa = false,
        CancellationToken cancellationToken = default);
}
