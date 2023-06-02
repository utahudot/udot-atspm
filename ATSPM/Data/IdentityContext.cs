using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class IdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }
}

public class ApplicationUser : IdentityUser
{
    // Additional properties for your custom user entity
    public string FirstName { get; set; }
    public string LastName { get; set; }

    // Navigation property for user roles
    public ICollection<IdentityUserRole<string>> Roles { get; set; }
}