using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var secret = builder.Configuration["SwaggerSecret"] ?? "";

// Serviços
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<BloqueioService>();
builder.Services.AddCors();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middlewares
// Basic auth protection for Swagger
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/swagger"))
    {
        // If no secret is configured, allow access
        if (string.IsNullOrEmpty(secret))
        {
            await next();
            return;
        }

        var auth = context.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Basic "))
        {
            try
            {
                var encoded = auth.Substring("Basic ".Length).Trim();
                var bytes = System.Convert.FromBase64String(encoded);
                var decoded = System.Text.Encoding.UTF8.GetString(bytes);
                // Expecting `username:password` where username can be anything (e.g. "swagger")
                var idx = decoded.IndexOf(':');
                if (idx >= 0)
                {
                    var password = decoded.Substring(idx + 1);
                    if (password == secret)
                    {
                        await next();
                        return;
                    }
                }
            }
            catch { /* ignore malformed header */ }
        }

        // Request Basic credentials
        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Swagger\"";
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }

    await next();
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();
app.MapHub<BloqueioHub>("/hub/bloqueio");
// Serve index.html for unmatched routes (SPA fallback)
app.MapFallbackToFile("index.html");

// Health endpoint for readiness/liveness checks
app.MapGet("/health", () => Results.Json(new { status = "ok" }));

app.Run();