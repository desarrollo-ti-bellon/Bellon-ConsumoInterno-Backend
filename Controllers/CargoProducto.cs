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
public class CargosProductoController : ControllerBase
{
    private readonly ILogger<CargosProductoController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioCargoProducto _servicioCargoProducto;

    public CargosProductoController(
        ILogger<CargosProductoController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioCargoProducto servicioCargoProducto
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioCargoProducto = servicioCargoProducto;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] string? id)
    {
        try
        {
            if (!string.IsNullOrEmpty(id))
            {
                var data = await _servicioCargoProducto.ObtenerCargoProducto(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioCargoProducto.ObtenerCargosProducto();
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }
}
