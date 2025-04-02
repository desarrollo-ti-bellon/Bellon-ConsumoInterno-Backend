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
public class EstadoSolicitudController : ControllerBase
{
    private readonly ILogger<EstadoSolicitudController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioEstadoSolicitud _servicioEstadoSolicitud;

    public EstadoSolicitudController(
        ILogger<EstadoSolicitudController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioEstadoSolicitud servicioEstadoSolicitud
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioEstadoSolicitud = servicioEstadoSolicitud;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        try
        {
            if (id.HasValue)
            {
                var data = await _servicioEstadoSolicitud.ObtenerEstadoSolicitud(id.Value);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioEstadoSolicitud.ObtenerEstadoSolicitudes();
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (Exception ex)
        {
            throw ex;
            // return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> GuardarLinea([FromBody] Classes.EstadoSolicitudCI item)
    {
        try
        {
            var result = await _servicioEstadoSolicitud.GuardarEstadoSolicitud(item);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            throw ex;
            // return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Eliminar([FromQuery] int id)
    {
        try
        {
            var result = await _servicioEstadoSolicitud.EliminarEstadoSolicitud(id);
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
            var result = await _servicioEstadoSolicitud.EliminarEstadoSolicitud(id);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            throw ex;
            // return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }
}
