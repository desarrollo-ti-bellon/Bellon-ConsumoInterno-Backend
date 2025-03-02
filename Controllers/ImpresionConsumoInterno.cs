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
public class ImpresionConsumoInternoController : ControllerBase
{
    private readonly ILogger<ImpresionConsumoInternoController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioImpresionConsumoInterno _servicioImpresionConsumoInterno;

    public ImpresionConsumoInternoController(
        ILogger<ImpresionConsumoInternoController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioImpresionConsumoInterno servicioImpresionConsumoInterno
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioImpresionConsumoInterno = servicioImpresionConsumoInterno;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener()
    {
        var data = await _servicioImpresionConsumoInterno.ObtenerImpresionConsumosInternos();
        return data != null ? Ok(data) : NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> ObtenerConFiltros([FromBody] FiltroGeneral filtros)
    {
        var data = await _servicioImpresionConsumoInterno.ObtenerImpresionConsumosInternosConFiltros(filtros);
        return data != null ? Ok(data) : NoContent();
    }
    
}
