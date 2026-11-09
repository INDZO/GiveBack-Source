using Microsoft.OpenApi;

namespace GiveBack.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddGiveBackSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "GiveBack API", Version = "v1" });
            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter JWT Bearer token.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };
            options.AddSecurityDefinition("Bearer", securityScheme);
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document, null)] = new List<string>()
            });
            options.OperationFilter<GiveBackSwaggerExamplesOperationFilter>();
        });
        return services;
    }

    public static WebApplication UseGiveBackSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "GiveBack API v1");
        });
        return app;
    }
}
