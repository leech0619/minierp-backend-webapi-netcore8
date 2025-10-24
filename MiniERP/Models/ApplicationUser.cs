using Microsoft.AspNetCore.Identity;

namespace MiniERP.Models;

/// <summary>
/// Custom user entity extending ASP.NET Identity.
/// Adds FirstName and LastName to the default Identity user.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
