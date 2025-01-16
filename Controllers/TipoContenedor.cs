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
public class TipoContenedoresController : ControllerBase
{
    private readonly ILogger<TipoContenedoresController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioTipoContenedor _servicioTipoContenedor;

    public TipoContenedoresController(
        ILogger<TipoContenedoresController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioTipoContenedor servicioTipoContenedor
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioTipoContenedor = servicioTipoContenedor;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        if (id.HasValue)
        {
            var data = await _servicioTipoContenedor.ObtenerTipoContenedor(id.Value);
            return data != null ? Ok(data) : NoContent();
        }
        else
        {
            var data = await _servicioTipoContenedor.ObtenerTipoContenedores();
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
    }

    [HttpGet("Activos")]
    public async Task<IActionResult> ObtenerActivos()
    {
        var data = await _servicioTipoContenedor.ObtenerTipoContenedoresActivos();
        return data != null && data.Count > 0 ? Ok(data) : NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Guardar([FromBody] Classes.TipoContenedor item)
    {
        var result = await _servicioTipoContenedor.GuardarTipoContenedor(item);
        return result != null ? Ok(result) : NoContent();
    }
}
