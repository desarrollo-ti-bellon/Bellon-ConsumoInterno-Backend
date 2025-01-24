using Azure;
using Bellon.API.Liquidacion.Authorization;
using Bellon.API.Liquidacion.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace Bellon.API.Liquidacion.Controllers;

[Authorize]
[AutorizacionBellon]
[RequiredScope("App.Liquidacion")]
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

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] string? id)
    {
        try
        {
            if (!string.IsNullOrEmpty(id))
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
}
