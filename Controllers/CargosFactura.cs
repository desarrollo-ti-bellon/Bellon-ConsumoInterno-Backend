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
public class CargosFacturaController : ControllerBase
{
    private readonly ILogger<CargosProductoController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioCargoFactura _servicioCargoFactura;

    public CargosFacturaController(
        ILogger<CargosProductoController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioCargoFactura servicioCargoFactura
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioCargoFactura = servicioCargoFactura;
    }

    [HttpGet("ERP")]
    public async Task<IActionResult> ObtenerCargoFacturaERP([FromQuery] string? id)
    {
        try
        {
            if (!string.IsNullOrEmpty(id))
            {
                var data = await _servicioCargoFactura.ObtenerCargoFacturaERP(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioCargoFactura.ObtenerCargoFacturasERP();
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int id)
    {
        try
        {
            if (id != null)
            {
                var data = await _servicioCargoFactura.ObtenerCargoFactura(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioCargoFactura.ObtenerCargoFacturas();
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Guardar([FromBody] List<Classes.CargoFacturaLiquidacion> item)
    {
        try
        {
            var result = await _servicioCargoFactura.GuardarCargoFactura(item);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Eliminar([FromQuery] int id)
    {
        try
        {
            var result = await _servicioCargoFactura.EliminarLineaCargoFactura(id);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

}
