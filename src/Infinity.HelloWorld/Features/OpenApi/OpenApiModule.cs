using Infinity.Toolkit.FeatureModules;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Reflection;

namespace Infinity.HelloWorld.Features.OpenApi;

public class OpenApiModule : IWebFeatureModule
{
    public IModuleInfo ModuleInfo { get; } = new FeatureModuleInfo(nameof(OpenApiModule), "1.0.0");

    public ModuleContext RegisterModule(ModuleContext context)
    {
        context.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

        context.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, serviceProvider, _) =>
            {
                document.Info.Title = context.Configuration["OpenApi:Info:Title"];
                document.Info.Version = $"Version {Assembly.GetExecutingAssembly().GetName().Version?.ToString()}";
                document.Info.Description = $"{context.Configuration["OpenApi:Info:Description"]} - Environment: {context.Environment.EnvironmentName}";

                return Task.CompletedTask;
            });

            var scheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Name = JwtBearerDefaults.AuthenticationScheme,
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            options.AddOperationTransformer((operation, context, ct) =>
            {
                if (context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any())
                {
                    operation.Security = [new() { [scheme] = [] }];
                }

                return Task.CompletedTask;
            });

            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Components ??= new();
                document.Components.SecuritySchemes.Add(JwtBearerDefaults.AuthenticationScheme, scheme);
                return Task.CompletedTask;
            });
        });

        return context;
    }

    public void MapEndpoints(WebApplication app, RouteGroupBuilder? groupBuilder = null)
    {
    }

    public void MapEndpoints(WebApplication builder)
    {
        builder.MapOpenApi();
        builder.MapScalarApiReference();
    }
}
