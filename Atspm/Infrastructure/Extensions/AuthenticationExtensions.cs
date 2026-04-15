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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models.IdentityModels;

namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
    /// <summary>
    /// Custom authentication handler that validates requests using an API key provided in the "X-API-KEY" header.
    /// </summary>
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        /// <summary>
        /// The database context used to access and validate API keys.
        /// </summary>
        private readonly IdentityContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyAuthenticationHandler"/> class.
        /// </summary>
        /// <param name="options">The monitor for the authentication scheme options.</param>
        /// <param name="logger">The logger factory for capturing diagnostic information.</param>
        /// <param name="encoder">The URL encoder for handling authentication-related strings.</param>
        /// <param name="context">The <see cref="IdentityContext"/> used for database operations.</param>
        public ApiKeyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, IdentityContext context) : base(options, logger, encoder)
        {
            _context = context;
        }

        /// <summary>
        /// Performs the core authentication logic by extracting the API key from the request header and verifying it against the database.
        /// </summary>
        /// <returns>
        /// An <see cref="AuthenticateResult"/> indicating success if the key is valid, active, and not expired; 
        /// otherwise, an appropriate failure result.
        /// </returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-API-KEY", out var extractedKey))
            {
                return AuthenticateResult.NoResult();
            }

            string providedKey = extractedKey.ToString();
            string hashedKey = ApiKeyGenerator.HashKey(providedKey);

            var apiKey = await _context.Set<ApiKey>()
                .Include(k => k.Claims)
                .FirstOrDefaultAsync(k => k.KeyHash == hashedKey
                                       && !k.IsRevoked
                                       && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow));

            if (apiKey == null)
            {
                return AuthenticateResult.Fail("Invalid, revoked, or expired API Key.");
            }
            var claimsList = new List<Claim>();

            if (apiKey.Claims != null)
            {
                foreach (var c in apiKey.Claims)
                {
                    claimsList.Add(new Claim(c.Type ?? ClaimTypes.Role, c.Value ?? ""));
                }
            }

            if (!string.IsNullOrEmpty(apiKey.OwnerId))
            {
                claimsList.Add(new Claim(ClaimTypes.NameIdentifier, apiKey.OwnerId));
            }

            claimsList.Add(new Claim("AuthenticationMethod", "ApiKey"));

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
                var schemes = new[] { JwtBearerDefaults.AuthenticationScheme, "ApiKey" };

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

                options.AddPolicy("CanCreateApiKeys", policy => {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == ClaimTypes.Role &&
                        (c.Value == "ApiKey:Create" || c.Value == "Admin")));
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
