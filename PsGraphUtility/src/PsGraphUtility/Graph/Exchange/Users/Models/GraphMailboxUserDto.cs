using System;
using System.Collections.Generic;

namespace PsGraphUtility.Graph.Exchange.Users.Models
{
    public sealed class GraphMailboxUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserPrincipalName { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? PrimarySmtpAddress { get; set; }

        public GraphMailboxSummaryDto? Mailbox { get; set; }

        public GraphMailboxStatisticsDto? Statistics { get; set; }

        public IReadOnlyList<GraphMailboxFolderStatisticsDto>? FolderStatistics { get; set; }

        public IReadOnlyList<GraphMailboxFolderPermissionDto>? FolderPermissions { get; set; }
    }

    public sealed class GraphMailboxSummaryDto
    {
        public string? RecipientTypeDetails { get; set; }
        public bool? ArchiveEnabled { get; set; }
        public string? ArchiveStatus { get; set; }
        public string? LitigationHoldEnabled { get; set; }
        public string? RetentionPolicy { get; set; }
    }

    public sealed class GraphMailboxStatisticsDto
    {
        public long? ItemCount { get; set; }
        public long? TotalItemSizeBytes { get; set; }
        public DateTimeOffset? LastLogonTime { get; set; }
        public DateTimeOffset? LastLogoffTime { get; set; }
        public DateTimeOffset? LastInteractionTime { get; set; }
    }

    public sealed class GraphMailboxFolderStatisticsDto
    {
        public string? FolderPath { get; set; }
        public long? ItemsInFolder { get; set; }
        public long? ItemsInFolderAndSubfolders { get; set; }
        public long? FolderSizeBytes { get; set; }
    }

    public sealed class GraphMailboxFolderPermissionDto
    {
        public string? FolderPath { get; set; }
        public string? User { get; set; }
        public string? AccessRights { get; set; }
        public bool IsInherited { get; set; }
    }
}
