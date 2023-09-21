using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
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

public class IdentityConfigurationContext : ConfigurationDbContext<IdentityConfigurationContext>
{
    public IdentityConfigurationContext(DbContextOptions<IdentityConfigurationContext> options, ConfigurationStoreOptions storeOptions)
        : base(options, storeOptions)
    { }
}

public class IdentityOperationalContext : PersistedGrantDbContext<IdentityOperationalContext>
{
    public IdentityOperationalContext(DbContextOptions<IdentityOperationalContext> options, OperationalStoreOptions storeOptions)
        : base(options, storeOptions)
    { }
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