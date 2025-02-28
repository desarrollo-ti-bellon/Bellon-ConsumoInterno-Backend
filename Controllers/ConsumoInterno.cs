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
public class ConsumoInternoController : ControllerBase
{
    private readonly ILogger<ConsumoInternoController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioConsumoInterno _servicioConsumoInterno;

    public ConsumoInternoController(
        ILogger<ConsumoInternoController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioConsumoInterno servicioConsumoInterno
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioConsumoInterno = servicioConsumoInterno;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        if (id.HasValue)
        {
            var data = await _servicioConsumoInterno.ObtenerConsumoInterno(id.Value);
            return data != null ? Ok(data) : NoContent();
        }
        else
        {
            var data = await _servicioConsumoInterno.ObtenerConsumosInternosSegunPosicionUsuario();
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> ObtenerConsumosInternosFiltrados([FromBody] FiltroGeneral filtro)
    {
        var data = await _servicioConsumoInterno.ObtenerConsumosInternosSegunPosicionUsuarioYFiltrosGenerales(filtro);
        return data != null && data.Count > 0 ? Ok(data) : NoContent();
    }

    [HttpGet("Cantidad")]
    public async Task<IActionResult> ObtenerEstadoConsumoInterno()
    {
        var result = await _servicioConsumoInterno.ObtenerCantidadConsumoInternos();
        return result != null ? Ok(result) : Ok(0);
    }

}