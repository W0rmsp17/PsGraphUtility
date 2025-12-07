using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;

namespace PsGraphUtility.Graph.Entra.Groups.Interface
{
    public interface IGroupLookupService
    {
        /// <summary>
        /// Accepts an id, displayName, mail, or mailNickname and returns the group id (GUID string).
        /// </summary>
        Task<string> ResolveGroupIdAsync(
            GraphAuthContext context,
            string idOrNameOrMail,
            CancellationToken cancellationToken = default);
    }
}
