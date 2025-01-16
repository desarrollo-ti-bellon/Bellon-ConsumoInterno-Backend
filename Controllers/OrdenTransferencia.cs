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
public class OrdenTransferenciaController : ControllerBase
{
    private readonly ILogger<OrdenTransferenciaController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioOrdenTransferencia _servicioOrdenTransferencia;

    public OrdenTransferenciaController(
        ILogger<OrdenTransferenciaController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioOrdenTransferencia servicioOrdenTransferencia
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioOrdenTransferencia = servicioOrdenTransferencia;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        if (id.HasValue)
        {
            var data = await _servicioOrdenTransferencia.ObtenerOrdenes(id.Value);
            return data != null ? Ok(data) : NoContent();
        }
        else
        {
            var data = await _servicioOrdenTransferencia.ObtenerOrdenes();
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
    }
}
