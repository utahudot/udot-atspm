using MOE.Common.Models;

namespace MOE.Common.Business.SiteSecurity
{
    public class IdentityConfig
    {
        public void Configuration(IAppBuilder app)
        {
            app.CreatePerOwinContext(() => new SPM());
            app.CreatePerOwinContext<SPMUserManager>(SPMUserManager.Create);
            app.CreatePerOwinContext<RoleManager<SPMRole>>((options, context) =>
                new RoleManager<SPMRole>(
                    new RoleStore<SPMRole>(context.Get<SPM>())));

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Home/Login")
            });
        }
    }
}