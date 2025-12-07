using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace PsGraphUtility.Graph.Entra.Devices.Models
{
    public sealed class GraphDeviceDto
    {
        public string Id { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? DeviceId { get; set; }

        public string? OperatingSystem { get; set; }
        public string? OperatingSystemVersion { get; set; }

        public bool? AccountEnabled { get; set; }
        public DateTimeOffset? ApproximateLastSignInDateTime { get; set; }

        public string? DeviceCategory { get; set; }
        public string? DeviceOwnership { get; set; }
        public string? TrustType { get; set; }

        public string? ManagementType { get; set; }
        public string? ComplianceState { get; set; }
    }
}
