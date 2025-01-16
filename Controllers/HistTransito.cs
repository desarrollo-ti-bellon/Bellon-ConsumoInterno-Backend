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
public class HistTransitosController : ControllerBase
{
    private readonly ILogger<HistTransitosController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioHistTransito _servicioHistTransito;

    public HistTransitosController(
        ILogger<HistTransitosController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioHistTransito servicioHistTransito
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioHistTransito = servicioHistTransito;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id, [FromQuery] int? idLiquidacion)
    {
        if (id.HasValue)
        {
            var data = await _servicioHistTransito.ObtenerTransito(id.Value);
            return data != null ? Ok(data) : NoContent();
        }
        else if (idLiquidacion.HasValue)
        {
            var data = await _servicioHistTransito.ObtenerTransitos(idLiquidacion.Value);
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
        else
        {
            var data = await _servicioHistTransito.ObtenerTransitos();
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
    }

    [HttpGet("Recuperar")]
    public async Task<IActionResult> Recuperar([FromQuery] int id)
    {
        var data = await _servicioHistTransito.Recuperar(id);
        return data.Exito ? Ok() : StatusCode(500, data);
    }
}
