using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
    /// <summary>
    /// Helper extensions for <see cref="Microsoft.Extensions.Hosting"/> using <see cref="Microsoft.AspNetCore.Authentication"/> 
    /// </summary>
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Adds Atspm identity services if <see cref="Host"/> is not in <see cref="Environments.Development"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IServiceCollection AddAtspmIdentity(this IServiceCollection services, HostBuilderContext host)
        {
            //if (!host.HostingEnvironment.IsDevelopment())
            //{
                services.AddAtspmAuthentication(host);
                services.AddAtspmAuthorization();
            //}
            
            return services;
        }
        
        /// <summary>
        /// Add atspm authentication
        /// </summary>
        /// <param name="services"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IServiceCollection AddAtspmAuthentication(this IServiceCollection services, HostBuilderContext host)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = host.Configuration["Jwt:Issuer"],
                    ValidAudience = host.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(host.Configuration["Jwt:Key"]))
                };
            });

            var oidc = host.Configuration.GetSection("Oidc");
            if (oidc.Exists() && !string.IsNullOrEmpty(oidc["Authority"]) &&
                !string.IsNullOrEmpty(oidc["ClientId"]) &&
                !string.IsNullOrEmpty(oidc["ClientSecret"]) &&
                !string.IsNullOrEmpty(oidc["RedirectUri"]))
            {
                services.AddAuthentication()
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = oidc["Authority"];
                options.ClientId = oidc["ClientId"];
                options.ClientSecret = oidc["ClientSecret"];
                options.ResponseType = OpenIdConnectResponseType.IdToken;
                options.SaveTokens = true;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("email");
                options.Scope.Add("profile");
                options.Scope.Add("app:Atspm");
                options.CallbackPath = "/api/Account/OIDCLoginCallback";
                options.GetClaimsFromUserInfoEndpoint = true;
                options.UseTokenLifetime = true;
                options.SkipUnrecognizedRequests = true;
                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        context.ProtocolMessage.RedirectUri = oidc["RedirectUri"];
                        return Task.CompletedTask;
                    },
                    OnTokenResponseReceived = context =>
                    {
                        var identity = context.Principal.Claims;
                        return Task.CompletedTask;
                    },
                    OnUserInformationReceived = context =>
                    {
                        var identity = context.Principal.Claims;
                        return Task.CompletedTask;
                    },
                    OnAuthorizationCodeReceived = context =>
                    {
                        var identity = context.Principal.Claims;
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var identity = context.Principal.Claims;
                        return Task.CompletedTask;
                    },
                };
            });
            }

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            return services;
        }

        /// <summary>
        /// Add atspm authorization
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAtspmAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanViewUsers", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            c.Type == ClaimTypes.Role && c.Value == "User:View" ||
                            c.Type == ClaimTypes.Role && c.Value == "Admin")));
                options.AddPolicy("CanEditUsers", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            c.Type == ClaimTypes.Role && c.Value == "User:Edit" ||
                            c.Type == ClaimTypes.Role && c.Value == "Admin")));
                options.AddPolicy("CanDeleteUsers", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            c.Type == ClaimTypes.Role && c.Value == "User:Delete" ||
                            c.Type == ClaimTypes.Role && c.Value == "Admin")));


                options.AddPolicy("CanViewRoles", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           c.Type == ClaimTypes.Role && c.Value == "Role:View" ||
                           c.Type == ClaimTypes.Role && c.Value == "Admin")));
                options.AddPolicy("CanEditRoles", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           c.Type == ClaimTypes.Role && c.Value == "Role:Edit" ||
                           c.Type == ClaimTypes.Role && c.Value == "Admin")));
                options.AddPolicy("CanDeleteRoles", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           c.Type == ClaimTypes.Role && c.Value == "Role:Delete" ||
                           c.Type == ClaimTypes.Role && c.Value == "Admin")));


                options.AddPolicy("CanViewLocationConfigurations", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           c.Type == ClaimTypes.Role && c.Value == "LocationConfiguration:View" ||
                           c.Type == ClaimTypes.Role && c.Value == "Admin")));
                options.AddPolicy("CanEditLocationConfigurations", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           c.Type == ClaimTypes.Role && c.Value == "LocationConfiguration:Edit" ||
                           c.Type == ClaimTypes.Role && c.Value == "Admin")));
                options.AddPolicy("CanDeleteLocationConfigurations", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           c.Type == ClaimTypes.Role && c.Value == "LocationConfiguration:Delete" ||
                           c.Type == ClaimTypes.Role && c.Value == "Admin")));


                options.AddPolicy("CanViewGeneralConfigurations", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           c.Type == ClaimTypes.Role && c.Value == "GeneralConfiguration:View" ||
                           c.Type == ClaimTypes.Role && c.Value == "Admin")));
                options.AddPolicy("CanEditGeneralConfigurations", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           c.Type == ClaimTypes.Role && c.Value == "GeneralConfiguration:Edit" ||
                           c.Type == ClaimTypes.Role && c.Value == "Admin")));
                options.AddPolicy("CanDeleteGeneralConfigurations", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           c.Type == ClaimTypes.Role && c.Value == "GeneralConfiguration:Delete" ||
                           c.Type == ClaimTypes.Role && c.Value == "Admin")));


                options.AddPolicy("CanViewData", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           c.Type == ClaimTypes.Role && c.Value == "Data:View" ||
                           c.Type == ClaimTypes.Role && c.Value == "Admin")));
                options.AddPolicy("CanEditData", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           c.Type == ClaimTypes.Role && c.Value == "Data:Edit" ||
                           c.Type == ClaimTypes.Role && c.Value == "Admin")));


                options.AddPolicy("CanViewWatchDog", policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c =>
                           c.Type == ClaimTypes.Role && c.Value == "Watchdog:View" ||
                           c.Type == ClaimTypes.Role && c.Value == "Admin")));
            });

            return services;
        }
    }
}
