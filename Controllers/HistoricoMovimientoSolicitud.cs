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
public class HistorialMovimientoSolicitudesCIController : ControllerBase
{
    private readonly ILogger<HistorialMovimientoSolicitudesCIController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioHistorialMovimientosSolicitudes _servicioSHistorialMovimientosSolicitudes;

    public HistorialMovimientoSolicitudesCIController(
        ILogger<HistorialMovimientoSolicitudesCIController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioHistorialMovimientosSolicitudes servicioHistorialMovimientosSolicitudes
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioSHistorialMovimientosSolicitudes = servicioHistorialMovimientosSolicitudes;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        try
        {
            if (id.HasValue)
            {
                var data = await _servicioSHistorialMovimientosSolicitudes.ObtenerHistorialMovimientoSolicitud(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioSHistorialMovimientosSolicitudes.ObtenerHistorialMovimientosSolicitudes();
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (InvalidDataException ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    [HttpGet("Agrupado")]
    public async Task<IActionResult> ObtenerAgrupados()
    {
        try
        {
            var data = await _servicioSHistorialMovimientosSolicitudes.ObtenerHistorialMovimientosSolicitudesAgrupados();
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
        catch (InvalidDataException ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    [HttpPost("Agrupado")]
    public async Task<IActionResult> ObtenerAgrupadosCOnFiltrosGenerales([FromBody] FiltroGeneral filtros)
    {
        try
        {
            var data = await _servicioSHistorialMovimientosSolicitudes.ObtenerHistorialMovimientosSolicitudesAgrupadosConFiltrosGenerales(filtros);
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
        catch (InvalidDataException ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    [HttpGet("Historial")]
    public async Task<IActionResult> ObtenerHistorico([FromQuery] string? id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Debe proporcionar el número de documento.");
            }
            var data = await _servicioSHistorialMovimientosSolicitudes.ObtenerHistorialMovimientosSolicitudes(id);
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
        catch (InvalidDataException ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    [HttpGet("Cantidad")]
    public async Task<IActionResult> ObtenerEstadoSolicitud([FromQuery] int? estadoSolicitudId)
    {
        try
        {
            var result = await _servicioSHistorialMovimientosSolicitudes.ObtenerCantidadHistorialMovimientoSolicitud();
            return Ok(result);
        }
        catch (InvalidDataException ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

}
