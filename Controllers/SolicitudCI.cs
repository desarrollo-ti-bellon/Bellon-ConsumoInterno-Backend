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

    [HttpGet("Solicitudes")]
    public async Task<IActionResult> ObtenerSolicitudes()
    {
        try
        {
            var data = await _servicioSolicitud.ObtenerSolicitudesPorPerfilUsuario();
            return data != null ? Ok(data) : NoContent();
        }
        catch (Exception ex)
        {
            throw ex;
            // return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpGet("EstadoSolicitud")]
    public async Task<IActionResult> ObtenerSolicitudesPorEstadoSolicitud([FromQuery] int? estadoSolicitudId)
    {
        try
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
        catch (Exception ex)
        {
            throw ex;
            // return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        try
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
        catch (Exception ex)
        {
            throw ex;
            // return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpGet("SolicitudGeneral")]
    public async Task<IActionResult> ObtenerSolicitudGeneral([FromQuery] int? id)
    {
        try
        {
            if (id.HasValue)
            {
                var data = await _servicioSolicitud.VerSolicitudesGenerales(id.Value);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                return StatusCode(400, new Classes.Resultado { Exito = false, Mensaje = "Falta el id de solicitud." });
            }
        }
        catch (Exception ex)
        {
            throw ex;
            // return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpPost("Cabecera")]
    public async Task<IActionResult> GuardarCabecera([FromBody] Classes.CabeceraSolicitudCI item)
    {
        try
        {
            var result = await _servicioSolicitud.GuardarSolicitud(item);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            throw ex;
            // return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
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
            throw ex;
            // return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }

    }

    [HttpPost("Linea")]
    public async Task<IActionResult> GuardarLinea([FromBody] List<Classes.LineasSolicitudCI> items)
    {
        try
        {
            var result = await _servicioSolicitud.GuardarLineasSolicitud(items);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            throw ex;
            // return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
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
            throw ex;
            // return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpGet("Cantidad")]
    public async Task<IActionResult> ObtenerEstadoSolicitud([FromQuery] int? estadoSolicitudId)
    {
        try
        {
            var result = await _servicioSolicitud.ObtenerCantidadSolicitudesPorEstadoSolicitud(estadoSolicitudId.Value);
            return result != null ? Ok(result) : Ok(0);
        }
        catch (Exception ex)
        {
            throw ex;
            // return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

}
