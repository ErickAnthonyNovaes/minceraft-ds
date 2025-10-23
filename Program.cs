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
// Password protection for Swagger
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/swagger"))
    {
        var supplied = context.Request.Query["senha"].ToString() ?? "";
        var cookie = context.Request.Cookies["SwaggerAuth"];

        // If the correct password was supplied, set a cookie so subsequent
        // requests for swagger assets (JS/CSS) are allowed without the query param.
        if (!string.IsNullOrEmpty(secret) && supplied == secret)
        {
            // Set a secure cookie (SameSite=None required for some cross-site scenarios)
            context.Response.Cookies.Append("SwaggerAuth", "1", new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Path = "/",
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None
            });

            // Redirect to the same path without query string so the browser will
            // request assets with the cookie included (avoids returning the HTML login for JS files).
            var redirectPath = context.Request.Path.HasValue ? context.Request.Path.Value : "/swagger";
            context.Response.Redirect(redirectPath);
            return;
        }

        // Allow if cookie is present
        if (cookie == "1")
        {
            await next();
            return;
        }

        // Otherwise render the password prompt (HTML)
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(@"
                <html>
                    <body style='font-family:sans-serif;display:flex;align-items:center;justify-content:center;height:100vh'>
                        <form method='get'>
                            <h3>Senha requerida</h3>
                            <input name='senha' type='password' autocomplete='off' style='padding:8px;margin:8px' />
                            <button style='padding:8px'>Entrar</button>
                        </form>
                    </body>
                </html>");
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