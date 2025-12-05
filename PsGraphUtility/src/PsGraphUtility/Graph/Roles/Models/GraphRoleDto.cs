using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsGraphUtility.Graph.Roles.Models
{
    public sealed class GraphRoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? RoleTemplateId { get; set; }
        public bool IsEnabled { get; set; }
    }
}

