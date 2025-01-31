using Azure;
using Bellon.API.ConsumoInterno.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace Bellon.API.ConsumoInterno.Controllers;

[Authorize]
[AutorizacionBellon]
[RequiredScope("App.ConsumoInterno")]
[ApiController]
[Route("[controller]")]
public class NotasController : ControllerBase
{
    private readonly ILogger<NotasController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioNotas _servicioNotas;

    public NotasController(
        ILogger<NotasController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioNotas servicioNotas
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioNotas = servicioNotas;
    }

    [HttpGet("Cantidad")]
    public async Task<IActionResult> ObtenerCantidadNotas([FromQuery] string usuarioDestino)
    {
        var data = await _servicioNotas.ObtenerCantidadDeNotas(usuarioDestino);
        return data > 0 ? Ok(data) : Ok(0);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerNotas([FromQuery] string? usuarioDestino, string? tipoDocumento)
    {
        try
        {
            var data = await _servicioNotas.ObtenerNotasPorParametros(usuarioDestino, tipoDocumento);
            return data != null ? Ok(data) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Guardar([FromBody] Classes.Notas item)
    {
        var result = await _servicioNotas.GuardarNotas(item);
        return result != null ? Ok(result) : NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Eliminar([FromQuery] int id)
    {
        var result = await _servicioNotas.EliminarNotas(id);
        return result != null ? Ok(result) : NoContent();
    }
}
