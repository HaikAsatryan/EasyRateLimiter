using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace EasyRateLimiter.Demo.Extensions;

public static class SwaggerExtension
{
    public static WebApplicationBuilder AddPandaSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            var version = "v1";
            options.SwaggerDoc(version, new OpenApiInfo
            {
                Version = version,
                Title = $"{AppDomain.CurrentDomain.FriendlyName}",
                Description =
                    "Powered by PandaTech LLC: Where precision meets innovation. Let's build the future, one endpoint at a time.",
                Contact = new OpenApiContact
                {
                    Name = "PandaTech LLC",
                    Email = "info@pandatech.it",
                    Url = new Uri("https://pandatech.it"),
                }
            });
            
        });
        return builder;
    }

    public static WebApplication UsePandaSwagger(this WebApplication app)
    {
        if (app.Environment.IsProduction()) return app;
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            // Specify the custom display name for the tab
            options.DocumentTitle = $"Swagger - {AppDomain.CurrentDomain.FriendlyName}";

            options.InjectStylesheet("/assets/css/panda-style.css");
            options.InjectJavascript("/assets/js/docs.js");
            options.DocExpansion(DocExpansion.None);
        });
        return app;
    }
}