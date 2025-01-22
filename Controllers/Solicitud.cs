using Bellon.API.Liquidacion.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace Bellon.API.Liquidacion.Controllers;

[Authorize]
[AutorizacionBellon]
[RequiredScope("App.Liquidacion")]
[ApiController]
[Route("[controller]")]
public class SolicitudController : ControllerBase
{
    private readonly ILogger<SolicitudController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioSolicitud _servicioSolicitud;

    public SolicitudController(
        ILogger<SolicitudController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioSolicitud servicioSolicitud
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioSolicitud = servicioSolicitud;
    }

    [HttpGet("EstadoSolicitud")]
    public async Task<IActionResult> ObtenerSolicitudesPorEstadoSolicitud([FromQuery] int? estadoSolicitudId)
    {
        if (estadoSolicitudId.HasValue)
        {
            var data = await _servicioSolicitud.ObtenerSolicitudesDelUsuarioSolicitantePorEstado(estadoSolicitudId.Value);
            return data != null ? Ok(data) : NoContent();
        }
        else
        {
            var data = await _servicioSolicitud.ObtenerSolicitudesPorEstadoSolicitud(null);
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        if (id.HasValue)
        {
            var data = await _servicioSolicitud.ObtenerSolicitud(id.Value);
            return data != null ? Ok(data) : NoContent();
        }
        else
        {
            var data = await _servicioSolicitud.ObtenerSolicitudes();
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
    }

    [HttpPost("Cabecera")]
    public async Task<IActionResult> GuardarCabecera([FromBody] Classes.CabeceraSolicitud item)
    {
        try
        {
            var result = await _servicioSolicitud.GuardarSolicitud(item);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Bellon.API.Liquidacion.Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Archivar([FromQuery] int id)
    {
        try
        {
            var result = await _servicioSolicitud.EliminarSolicitud(id);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Bellon.API.Liquidacion.Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }

    }

    [HttpPost("Linea")]
    public async Task<IActionResult> GuardarLinea([FromBody] List<Classes.LineasSolicitud> items)
    {
        try
        {
            var result = await _servicioSolicitud.GuardarLineasSolicitud(items);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Bellon.API.Liquidacion.Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpDelete("Linea")]
    public async Task<IActionResult> EliminarLinea([FromQuery] int id)
    {
        try
        {
            var result = await _servicioSolicitud.EliminarLineaSolicitud(id);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Bellon.API.Liquidacion.Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpGet("Cantidad")]
    public async Task<IActionResult> ObtenerEstadoSolicitud([FromQuery] int? estadoSolicitudId)
    {
        try
        {
            var result = await _servicioSolicitud.ObtenerCantidadSolicitudesPorEstadoSolicitud(estadoSolicitudId ?? 0);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Bellon.API.Liquidacion.Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }
}
