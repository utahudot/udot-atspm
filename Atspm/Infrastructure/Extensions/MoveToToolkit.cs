using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Security.Cryptography;
using System.Text;

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

    public static class StuffToMove
    {
        public static SwaggerGenOptions AddAtspmSecurityDefinitions(this SwaggerGenOptions swaggerGenOptions)
        {
            // 1. Define the JWT Scheme
            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name = "JWT Authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            // 2. Define the API Key Scheme
            var apiKeySecurityScheme = new OpenApiSecurityScheme
            {
                Name = "X-API-KEY", // The actual header name the code looks for
                Description = "Enter your API Key directly (no 'Bearer' prefix needed)",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKey",
                Reference = new OpenApiReference
                {
                    Id = "ApiKey", // This ID is used for the requirement below
                    Type = ReferenceType.SecurityScheme
                }
            };

            // 3. Register both definitions
            swaggerGenOptions.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
            swaggerGenOptions.AddSecurityDefinition(apiKeySecurityScheme.Reference.Id, apiKeySecurityScheme);

            // 4. Require BOTH for all operations
            // Swagger will allow EITHER to satisfy the requirement if the user provides one
            swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() },
        { apiKeySecurityScheme, Array.Empty<string>() }
    });

            return swaggerGenOptions;
        }
    }
}
