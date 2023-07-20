using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class IdentityContext : IdentityDbContext<ApplicationUser>
{
    public IdentityContext(DbContextOptions<IdentityContext> options)
        : base(options)
    {

    }

}

public class ApplicationUser : IdentityUser
{
    // Additional properties for your custom user entity
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Agency { get; set; }
    public string FullName { get { return $"{FirstName} {LastName}"; } }

    // Navigation property for user roles
    public ICollection<IdentityUserRole<string>> Roles { get; set; }
}