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
public class HistLiquidacionesController : ControllerBase
{
    private readonly ILogger<HistLiquidacionesController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioHistLiquidacion _servicioHistLiquidacion;

    public HistLiquidacionesController(
        ILogger<HistLiquidacionesController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioHistLiquidacion servicioHistLiquidacion
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioHistLiquidacion = servicioHistLiquidacion;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        if (id.HasValue)
        {
            var data = await _servicioHistLiquidacion.ObtenerLiquidacion(id.Value);
            return data != null ? Ok(data) : NoContent();
        }
        else
        {
            var data = await _servicioHistLiquidacion.ObtenerLiquidaciones();
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
    }

    [HttpGet("Recuperar")]
    public async Task<IActionResult> Recuperar([FromQuery] int id)
    {
        var data = await _servicioHistLiquidacion.Recuperar(id);
        return data.Exito ? Ok() : StatusCode(500, data);
    }
}
