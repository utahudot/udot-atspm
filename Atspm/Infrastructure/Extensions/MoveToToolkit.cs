#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Extensions/MoveToToolkit.cs
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

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
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

    public static class TempHelpers
    {
        public static Task WhenInitialized(this ISupportInitializeNotification service)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (service.IsInitialized) return Task.CompletedTask;

            EventHandler handler = null;
            handler = (s, e) =>
            {
                service.Initialized -= handler;
                tcs.TrySetResult(true);
            };

            service.Initialized += handler;

            if (service.IsInitialized)
            {
                service.Initialized -= handler;
                tcs.TrySetResult(true);
            }

            return tcs.Task;
        }
    }
}
