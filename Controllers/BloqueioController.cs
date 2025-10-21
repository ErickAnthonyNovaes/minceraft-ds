using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]
public class BloqueioController : ControllerBase
{
    private readonly BloqueioService _service;
    private readonly IHubContext<BloqueioHub> _hub;

    public BloqueioController(BloqueioService service, IHubContext<BloqueioHub> hub)
    {
        _service = service;
        _hub = hub;
    }

    [HttpPost("alternar")]
    public async Task<IActionResult> Alternar()
    {
        _service.EstaBloqueado = !_service.EstaBloqueado;
        await _hub.Clients.All.SendAsync("EstadoAlterado", _service.EstaBloqueado);
        return Ok(new { bloqueado = _service.EstaBloqueado });
    }

    [HttpGet("estado")]
    public IActionResult Estado()
    {
        return Ok(new { bloqueado = _service.EstaBloqueado });
    }

    [HttpGet("/conteudo")]
    public IActionResult Conteudo()
    {
        if (_service.EstaBloqueado)
            return StatusCode(403);

        var html = @"
    <!doctype html>
    <html lang='pt-BR'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Conteúdo Protegido</title>
        <style>
            html, body {
                margin: 0;
                padding: 0;
                height: 100%;
                overflow: hidden;
            }
            iframe {
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                border: none;
            }
        </style>
    </head>
    <body>
        <iframe src='https://sd592g.github.io/zj684od4lfg/' allowfullscreen></iframe>
    </body>
    </html>
    ";

        return Content(html, "text/html");
    }
}