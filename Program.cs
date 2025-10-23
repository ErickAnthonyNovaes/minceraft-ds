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
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            var supplied = context.Request.Query["senha"].ToString() ?? "";

            if (secret != supplied)
            {
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
        }
        await next();
    });

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.RoutePrefix = "swagger";
    });
}

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