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
public class ClasificacionController : ControllerBase
{
    private readonly ILogger<ClasificacionController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioClasificacion _servicioClasificacion;

    public ClasificacionController(
        ILogger<ClasificacionController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioClasificacion servicioClasificacion
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioClasificacion = servicioClasificacion;
    }

    [HttpGet("ERP")]
    public async Task<IActionResult> ObtenerERP([FromQuery] string? id)
    {
        try
        {
            if (!string.IsNullOrEmpty(id))
            {
                var data = await _servicioClasificacion.ObtenerClasificacionERP(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioClasificacion.ObtenerClasificacionesERP();
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        try
        {
            if (id != null)
            {
                var data = await _servicioClasificacion.ObtenerClasificacion(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioClasificacion.ObtenerClasificaciones();
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpGet("Activas")]
    public async Task<IActionResult> ObtenerActivas()
    {
        try
        {
            var data = await _servicioClasificacion.ObtenerClasificacionesActivas();
            return data != null ? Ok(data) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Guardar([FromBody] Classes.ClasificacionCI? item)
    {
        try
        {
            var result = await _servicioClasificacion.GuardarClasificacion(item);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Eliminar([FromQuery] int id)
    {
        try
        {
            var data = await _servicioClasificacion.EliminarClasificacion(id);
            return Ok(data) ?? StatusCode(500, data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

}
