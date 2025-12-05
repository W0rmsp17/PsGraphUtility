using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Users.Models;

namespace PsGraphUtility.Graph.Users.Interface;

public interface IGetUserService
{
    Task<GraphUserDto> GetUserAsync(
        GraphAuthContext context,
        string userIdOrUpn,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GraphUserDto>> ListUsersAsync(
        GraphAuthContext context,
        CancellationToken cancellationToken = default);
}
