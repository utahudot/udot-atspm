#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Extensions/AuthenticationExtensions.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;          // For ILoggerFactory
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Utah.Udot.Atspm.Data;

namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
    public static class ApiKeyGenerator
    {
        public static string HashKey(string rawKey)
        {
            var inputBytes = Encoding.UTF8.GetBytes(rawKey);
            var hashBytes = SHA256.HashData(inputBytes);
            return Convert.ToBase64String(hashBytes);
        }

        public static (string RawKey, string Hash) CreateKey()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            string rawKey = Convert.ToBase64String(bytes)
                .Replace("/", "").Replace("+", "").Replace("=", "");

            return (rawKey, HashKey(rawKey));
        }
    }

    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IdentityContext _context; // Use your specific context name
        private const string ApiKeyHeaderName = "X-API-KEY";

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IdentityContext context) : base(options, logger, encoder)
        {
            _context = context;
        }

        protected override async System.Threading.Tasks.Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-API-KEY", out var extractedKey))
            {
                return Microsoft.AspNetCore.Authentication.AuthenticateResult.NoResult();
            }

            string providedKey = extractedKey.ToString();
            string hashedKey = ApiKeyGenerator.HashKey(providedKey);

            // Using Set<> to avoid any property-level naming conflicts in your Context
            var apiKey = await _context.Set<ApiKey>()
                .Include(k => k.Claims)
                .FirstOrDefaultAsync(k => k.KeyHash == hashedKey && !k.IsRevoked);

            if (apiKey == null || (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt < System.DateTime.UtcNow))
            {
                return AuthenticateResult.Fail("Invalid or expired API Key.");
            }

            var claimsList = new List<Claim>();

            // We get the Constructor Info for the Claim class manually
            var claimType = typeof(Claim);
            var ctor = claimType.GetConstructor(new[] { typeof(string), typeof(string) });

            if (apiKey.Claims != null)
            {
                foreach (var c in apiKey.Claims)
                {
                    // We 'Invoke' the constructor instead of using 'new'
                    var cObj = (System.Security.Claims.Claim)ctor.Invoke(new object[] { c.Type ?? "role", c.Value ?? "" });
                    claimsList.Add(cObj);
                }
            }

            // Add NameIdentifier and Method using the same 'Invoke' strategy to stay safe
            string nameIdUri = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
            claimsList.Add((Claim)ctor.Invoke(new object[] { nameIdUri, apiKey.OwnerId ?? "" }));
            claimsList.Add((Claim)ctor.Invoke(new object[] { "AuthenticationMethod", "ApiKey" }));

            // Build the rest using full paths
            var identity = new ClaimsIdentity(claimsList, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }



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
            })










            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);
















            var oidc = host.Configuration.GetSection("Oidc");
            if (oidc.Exists() && !string.IsNullOrEmpty(oidc["Authority"]) &&
                !string.IsNullOrEmpty(oidc["ClientId"]) &&
                !string.IsNullOrEmpty(oidc["ClientSecret"]) &&
                !string.IsNullOrEmpty(oidc["CallbackPath"]))
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

                options.CallbackPath = oidc["CallbackPath"];

                options.GetClaimsFromUserInfoEndpoint = true;
                options.UseTokenLifetime = true;
                options.SkipUnrecognizedRequests = true;

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        var b = new UriBuilder(context.ProtocolMessage.RedirectUri);
                        b.Scheme = "https";
                        b.Port = -1;
                        context.ProtocolMessage.RedirectUri = b.ToString();

                        Console.WriteLine($"callback: {b.ToString()}");

                        return Task.CompletedTask;
                    },
                    //OnTokenResponseReceived = context =>
                    //{
                    //    var identity = context.Principal.Claims;
                    //    return Task.CompletedTask;
                    //},
                    //OnUserInformationReceived = context =>
                    //{
                    //    var identity = context.Principal.Claims;
                    //    return Task.CompletedTask;
                    //},
                    //OnAuthorizationCodeReceived = context =>
                    //{
                    //    var identity = context.Principal.Claims;
                    //    return Task.CompletedTask;
                    //},
                    //OnTokenValidated = context =>
                    //{
                    //    var identity = context.Principal.Claims;
                    //    return Task.CompletedTask;
                    //},
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
                // helper string array to avoid typos
                var schemes = new[] { Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, "ApiKey" };

                options.AddPolicy("CanViewUsers", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "User:View" || c.Value == "Admin")));
                });

                options.AddPolicy("CanEditUsers", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "User:Edit" || c.Value == "Admin")));
                });

                options.AddPolicy("CanDeleteUsers", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "User:Delete" || c.Value == "Admin")));
                });

                options.AddPolicy("CanViewRoles", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "Role:View" || c.Value == "Admin")));
                });

                options.AddPolicy("CanEditRoles", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "Role:Edit" || c.Value == "Admin")));
                });

                options.AddPolicy("CanDeleteRoles", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "Role:Delete" || c.Value == "Admin")));
                });

                options.AddPolicy("CanViewLocationConfigurations", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "LocationConfiguration:View" || c.Value == "Admin")));
                });

                options.AddPolicy("CanEditLocationConfigurations", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "LocationConfiguration:Edit" || c.Value == "Admin")));
                });

                options.AddPolicy("CanDeleteLocationConfigurations", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "LocationConfiguration:Delete" || c.Value == "Admin")));
                });

                options.AddPolicy("CanViewGeneralConfigurations", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "GeneralConfiguration:View" || c.Value == "Admin")));
                });

                options.AddPolicy("CanEditGeneralConfigurations", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "GeneralConfiguration:Edit" || c.Value == "Admin")));
                });

                options.AddPolicy("CanDeleteGeneralConfigurations", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "GeneralConfiguration:Delete" || c.Value == "Admin")));
                });

                options.AddPolicy("CanViewData", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "Data:View" || c.Value == "Admin")));
                });

                options.AddPolicy("CanEditData", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "Data:Edit" || c.Value == "Admin")));
                });

                options.AddPolicy("CanViewWatchDog", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "Watchdog:View" || c.Value == "Admin")));
                });
            });

            return services;
        }
    }
}
