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
public class LiquidacionesController : ControllerBase
{
    private readonly ILogger<LiquidacionesController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioLiquidacion _servicioLiquidacion;

    public LiquidacionesController(
        ILogger<LiquidacionesController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioLiquidacion servicioLiquidacion
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioLiquidacion = servicioLiquidacion;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        if (id.HasValue)
        {
            var data = await _servicioLiquidacion.ObtenerLiquidacion(id.Value);
            return data != null ? Ok(data) : NoContent();
        }
        else
        {
            var data = await _servicioLiquidacion.ObtenerLiquidaciones();
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
    }

    [HttpPost("Cabecera")]
    public async Task<IActionResult> GuardarCabecera([FromBody] Classes.CabeceraLiquidacion item)
    {
        try
        {
            var result = await _servicioLiquidacion.GuardarCabeceraLiquidacion(item);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Archivar([FromQuery] int id)
    {
        try
        {
            var data = await _servicioLiquidacion.Archivar(id);
            return data.Exito ? Ok() : StatusCode(500, data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpPost("Linea")]
    public async Task<IActionResult> GuardarLinea([FromBody] List<Classes.LineaLiquidacion> items)
    {
        try
        {
            var result = await _servicioLiquidacion.GuardarLineaLiquidacion(items);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpDelete("Linea")]
    public async Task<IActionResult> EliminarLinea([FromQuery] int id)
    {
        try
        {
            var result = await _servicioLiquidacion.EliminarLineaLiquidacion(id);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpPost("CargoAdicional")]
    public async Task<IActionResult> GuardarCargoAdicional(
        [FromBody] List<Classes.CargoAdicional> items
    )
    {
        try
        {
            var result = await _servicioLiquidacion.GuardarCargoAdicionalLiquidacion(items);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpDelete("CargoAdicional")]
    public async Task<IActionResult> EliminarCargoAdicional([FromQuery] int id)
    {
        try
        {
            var result = await _servicioLiquidacion.EliminarCargoAdicionalLiquidacion(id);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpGet("Cantidad")]
    public async Task<IActionResult> ObtenerActivas()
    {
        try
        {
            var result = await _servicioLiquidacion.ObtenerActivas();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpPost("NuevasLineas")]
    public async Task<IActionResult> ObtenerLineas([FromBody] List<int> transitos)
    {
        try
        {
            var result = await _servicioLiquidacion.ObtenerLineasLiquidacion(transitos);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpPost("Transferir")]
    public async Task<IActionResult> CrearTransferencias([FromQuery] int id)
    {
        try
        {
            var data = await _servicioLiquidacion.CrearTransferencias(id);
            return data.Exito ? Ok() : StatusCode(500, data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }
}
