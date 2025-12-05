using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Users.Models;

namespace PsGraphUtility.Graph.Users.Interface;

public interface ISetUserService
{
    Task<GraphUserDto> UpdateUserAsync(
        GraphAuthContext context,
        string userIdOrUpn,
        string? displayName = null,
        string? mail = null,
        string? jobTitle = null,
        string? mobilePhone = null,
        string? officeLocation = null,
        string[]? businessPhones = null,
        string? password = null,
        bool? forceChangePassword = null,
        bool? blockSignIn = null,
        CancellationToken cancellationToken = default);
}
