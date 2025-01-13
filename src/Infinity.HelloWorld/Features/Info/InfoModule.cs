using Infinity.Toolkit.FeatureModules;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Infinity.HelloWorld.Features.Info;

public class InfoModule : WebFeatureModule
{
    public override IModuleInfo? ModuleInfo { get; }

    public override void MapEndpoints(WebApplication builder) => builder.MapInfoEndpoints();

    public override ModuleContext RegisterModule(ModuleContext featureModuleContext) => featureModuleContext;
}

public static class InfoEndpoints
{
    private static readonly string[] ForbiddenKeys = ["ConnectionString", "Auth", "Secret"];

    public static RouteGroupBuilder MapInfoEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/info")
            .WithTags("Info");

        group.MapGet("/version", GetVersion)
            .WithName("GetVersion")
            .WithDisplayName("Get service version")
            .Produces<VersionResponse>()
            .WithOpenApi();

        group.MapGet("/title", (IConfiguration configuration) => configuration.GetValue<string>("OpenApi:Info:Title") ?? Assembly.GetEntryAssembly()?.GetName().Name ?? "Title");
        group.MapGet("/config", GetConfig)
            .WithName("GetConfig")
            .WithDisplayName("Get service configuration")
            .RequireAuthorization();

        return group;
    }

    private static ContentHttpResult GetConfig(IConfiguration configuration)
    {
        var configInfo = (configuration as IConfigurationRoot)!.GetDebugView(context => context switch
        {
            { Key: var key } => ForbiddenKeys.Any(t => key.Contains(t, StringComparison.InvariantCultureIgnoreCase)) ? "******" : context.Value!,
        });

        return TypedResults.Text(configInfo);
    }

    private static JsonHttpResult<VersionResponse> GetVersion(IWebHostEnvironment webHostEnvironment)
    {
        return TypedResults.Json(new VersionResponse
        {
            Name = Assembly.GetEntryAssembly()?.GetName().Name ?? webHostEnvironment.ApplicationName ?? "Name",
            Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0",
            DateTime = DateTimeOffset.Now.UtcDateTime,
            Environment = webHostEnvironment.EnvironmentName,
            FrameworkVersion = Environment.Version.ToString(),
            OSVersion = Environment.OSVersion.ToString(),
            BuildDate = File.GetLastWriteTime(Assembly.GetEntryAssembly()!.Location).ToString("yyyy-MM-dd HH:mm:ss"),
            Host = Environment.MachineName,
            ProcessorArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            FrameworkDescription = RuntimeInformation.FrameworkDescription,
            RuntimeIdentifier = RuntimeInformation.RuntimeIdentifier,
            OSArchitecture = RuntimeInformation.OSArchitecture.ToString(),
            OSDescription = RuntimeInformation.OSDescription
        });
    }

    private record VersionResponse()
    {
        public string? Name { get; init; }

        public string? Version { get; init; }

        public DateTimeOffset DateTime { get; init; }

        public string? Environment { get; init; }

        public string? FrameworkVersion { get; init; }

        public string? OSVersion { get; init; }

        public string? BuildDate { get; init; }

        public string? Host { get; init; }

        public string? ProcessorArchitecture { get; init; }

        public string? FrameworkDescription { get; init; }

        public string? RuntimeIdentifier { get; init; }

        public string? OSArchitecture { get; init; }

        public string? OSDescription { get; init; }
    }
}
