using Infinity.Toolkit.FeatureModules;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedPrefix;
});

builder.AddFeatureModules();
builder.Services.AddHealthChecks();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseStatusCodePages();
}

app.UseForwardedHeaders();
app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-Forwarded-Prefix", out var prefix))
    {
        context.Response.Headers.Append("X-Forwarded-Prefix", new[] { prefix.ToString() });
    }

    await next(context);
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHealthChecks("/health");
app.MapFeatureModules();

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
