using Azure;
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
public class CodigosArancelarioController : ControllerBase
{
    private readonly ILogger<CodigosArancelarioController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioCodigoArancelario _servicioCodigoArancelario;

    public CodigosArancelarioController(
        ILogger<CodigosArancelarioController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioCodigoArancelario servicioCodigoArancelario
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioCodigoArancelario = servicioCodigoArancelario;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id, [FromQuery] string? idApi)
    {
        if (id.HasValue)
        {
            var data = await _servicioCodigoArancelario.ObtenerCodigoArancelario(id.Value);
            return data != null ? Ok(data) : NoContent();
        }
        else if (!string.IsNullOrEmpty(idApi))
        {
            var data = await _servicioCodigoArancelario.ObtenerPorcentajesCodigoArancelario(idApi);
            return data != null ? Ok(data) : NoContent();
        }
        else
        {
            var data = await _servicioCodigoArancelario.ObtenerCodigoArancelarios();
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
    }

    [HttpGet("ERP")]
    public async Task<IActionResult> ObtenerERP([FromQuery] string? id)
    {
        try
        {
            if (!string.IsNullOrEmpty(id))
            {
                var data = await _servicioCodigoArancelario.ObtenerCodigoArancelarioERP(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioCodigoArancelario.ObtenerCodigosArancelarioERP();
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Guardar([FromBody] Classes.CodigoArancelario item)
    {
        var result = await _servicioCodigoArancelario.GuardarCodigoArancelario(item);
        return result != null ? Ok(result) : NoContent();
    }
}
