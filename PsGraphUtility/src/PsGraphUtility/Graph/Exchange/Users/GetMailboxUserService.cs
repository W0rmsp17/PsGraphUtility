using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Exchange;
using PsGraphUtility.Graph;
using PsGraphUtility.Graph.Entra.Users.Interface;
using PsGraphUtility.Graph.Exchange.Users.Interface;
using PsGraphUtility.Graph.Exchange.Users.Models;
using PsGraphUtility.PowerShell.Exchange;
using PsGraphUtility.Graph.Interface;
using PsGraphUtility.Auth.Powershell;

namespace PsGraphUtility.Graph.Exchange.Users
{
    public sealed class GetMailboxUserService : IGetMailboxUserService
    {
        private readonly IGraphClient _graph;
        private readonly IUserLookupService _userLookup;

        public GetMailboxUserService(IGraphClient graph, IUserLookupService userLookup)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _userLookup = userLookup ?? throw new ArgumentNullException(nameof(userLookup));
        }

        public async Task<GraphMailboxUserDto> GetMailboxUserAsync(
            GraphAuthContext context,
            string userKey,
            bool includeMailbox,
            bool includeMailboxStats,
            bool includeFolderStats,
            bool includeFolderPermissions,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userKey))
                throw new ArgumentException("User key must be provided.", nameof(userKey));

            var dto = new GraphMailboxUserDto
            {
                UserPrincipalName = userKey
            };

            await ExchangeSessionManager.RunInExchangeSessionAsync(ps =>
            {
      
                if (includeMailbox)
                {
                    ps.Commands.Clear();
                    ps.AddCommand("Get-EXOMailbox")
                      .AddParameter("UserPrincipalName", userKey);

                    var mbx = ps.Invoke().FirstOrDefault();
                    ps.Commands.Clear();

                    if (mbx != null)
                    {
                        dto.Id = mbx.Members["ExternalDirectoryObjectId"]?.Value?.ToString() ?? dto.Id;
                        dto.DisplayName = mbx.Members["DisplayName"]?.Value?.ToString() ?? dto.DisplayName;
                        dto.PrimarySmtpAddress = mbx.Members["PrimarySmtpAddress"]?.Value?.ToString()
                                                 ?? dto.PrimarySmtpAddress;

                        var archiveStatus = mbx.Members["ArchiveStatus"]?.Value?.ToString();

                        dto.Mailbox = new GraphMailboxSummaryDto
                        {
                            RecipientTypeDetails = mbx.Members["RecipientTypeDetails"]?.Value?.ToString(),
                            ArchiveStatus = archiveStatus,
                            ArchiveEnabled = archiveStatus is null
                                ? null
                                : !string.Equals(archiveStatus, "None", StringComparison.OrdinalIgnoreCase),
                            LitigationHoldEnabled = mbx.Members["LitigationHoldEnabled"]?.Value?.ToString(),
                            RetentionPolicy = mbx.Members["RetentionPolicy"]?.Value?.ToString()
                        };
                    }
                }

       
                if (includeMailboxStats)
                {
                    ps.Commands.Clear();
                    ps.AddCommand("Get-EXOMailboxStatistics")
                      .AddParameter("UserPrincipalName", userKey);

                    var stat = ps.Invoke().FirstOrDefault();
                    ps.Commands.Clear();

                    if (stat != null)
                    {
                        dto.Statistics = new GraphMailboxStatisticsDto
                        {
                            ItemCount = stat.Members["ItemCount"]?.Value as long?,
                            LastLogonTime = stat.Members["LastLogonTime"]?.Value as DateTimeOffset?,
                            LastLogoffTime = stat.Members["LastLogoffTime"]?.Value as DateTimeOffset?,
                            LastInteractionTime = stat.Members["LastInteractionTime"]?.Value as DateTimeOffset?
                        };
                       
                    }
                }

            
                if (includeFolderStats)
                {
                    ps.Commands.Clear();
                    ps.AddCommand("Get-EXOMailboxFolderStatistics")
                      .AddParameter("UserPrincipalName", userKey);

                    var results = ps.Invoke();
                    ps.Commands.Clear();

                    var list = new List<GraphMailboxFolderStatisticsDto>();

                    foreach (var r in results)
                    {
                        list.Add(new GraphMailboxFolderStatisticsDto
                        {
                            FolderPath = r.Members["FolderPath"]?.Value?.ToString(),
                            ItemsInFolder = r.Members["ItemsInFolder"]?.Value as long?,
                            ItemsInFolderAndSubfolders =
                                r.Members["ItemsInFolderAndSubfolders"]?.Value as long?
                        });
                    }

                    dto.FolderStatistics = list;
                }

                if (includeFolderPermissions)
                {
                    ps.Commands.Clear();
                    ps.AddCommand("Get-EXOMailboxFolderPermission")
                      .AddParameter("Identity", $"{userKey}:\\Inbox");

                    var results = ps.Invoke();
                    ps.Commands.Clear();

                    var list = new List<GraphMailboxFolderPermissionDto>();

                    foreach (var r in results)
                    {
                        list.Add(new GraphMailboxFolderPermissionDto
                        {
                            FolderPath = r.Members["Identity"]?.Value?.ToString(),
                            User = r.Members["User"]?.Value?.ToString(),
                            AccessRights = r.Members["AccessRights"]?.Value?.ToString(),
                            IsInherited = (bool?)r.Members["IsInherited"]?.Value ?? false
                        });
                    }

                    dto.FolderPermissions = list;
                }

                return Task.CompletedTask;
            });

            return dto;
        }
    }
}
