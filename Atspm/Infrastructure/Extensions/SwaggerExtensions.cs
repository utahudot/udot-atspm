using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
    /// <summary>
    /// <see cref="Swashbuckle.AspNetCore"/> helper extensions
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Gets assembly from <paramref name="type"/> and inject human-friendly descriptions for Operations, Parameters and Schemas based on XML Comment files
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SwaggerGenOptions IncludeXmlComments(this SwaggerGenOptions swaggerGenOptions, Type type)
        {
            var fileName = type.Assembly.GetName().Name + ".xml";
            var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            swaggerGenOptions.IncludeXmlComments(filePath, false);

            return swaggerGenOptions;
        }

        /// <summary>
        /// Extracts the controller, action verb and action name and provides a custom formatter for the operationId.
        /// First path param is prefixed with <c>From</c> and subsequent path params are prefixed with <c>And</c>.
        /// If there is a <c>$count</c> param in the path then a suffix of <c>Count</c> is added.
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="operationIdFormat">Custom format function with inputs of controller, action verb and action name</param>
        /// <returns></returns>
        public static SwaggerGenOptions CustomOperationIds(this SwaggerGenOptions swaggerGenOptions, Func<string, string, string, string> operationIdFormat)
        {
            string controller = string.Empty;
            string verb = string.Empty;
            string action = string.Empty;
            
            swaggerGenOptions.CustomOperationIds(a =>
            {
                StringBuilder builder = new StringBuilder();

                controller = a.ActionDescriptor.RouteValues["controller"];
                action = a.ActionDescriptor.RouteValues["action"];

                foreach (var v in new List<string>() { "Get", "Put", "Post", "Patch", "Delete", "Upsert" })
                {
                    if (action.Contains(v))
                    {
                        verb = v;
                        action = action.Replace(v, "");
                    }
                }

                //builder.Append(verb);
                //builder.Append(controller);
                //builder.Append(action);

                builder.Append(operationIdFormat.Invoke(controller, verb, action));

                if (a.RelativePath.Contains("$count"))
                    builder.Append("Count");

                foreach (var id in a.ParameterDescriptions.Where(w => w.Source == BindingSource.Path).ToList())
                {
                    if (!builder.ToString().Contains("From"))
                        builder.Append("From");
                    else
                        builder.Append("And");

                    builder.Append(id.Name.Capitalize());
                }

                return builder.ToString();
            });

            return swaggerGenOptions;
        }

        public static SwaggerGenOptions AddJwtAuthorization(this SwaggerGenOptions swaggerGenOptions)
        {
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

            swaggerGenOptions.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

            swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtSecurityScheme, Array.Empty<string>() }
            });

            return swaggerGenOptions;
        }
    }
}
