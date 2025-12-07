using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// src/PsGraphUtility/Graph/Users/Models/GraphUserDto.cs
using System;

namespace PsGraphUtility.Graph.Entra.Users.Models;
public sealed class GraphUserDto
{
    public string Id { get; init; } = string.Empty;
    public string UserPrincipalName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Mail { get; init; }
    public string? JobTitle { get; init; }
    public string? MobilePhone { get; init; }
    public string? OfficeLocation { get; init; }
    public string[] BusinessPhones { get; init; } = Array.Empty<string>();
}

