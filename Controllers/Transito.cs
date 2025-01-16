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
public class TransitosController : ControllerBase
{
    private readonly ILogger<TransitosController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioTransito _servicioTransito;

    public TransitosController(
        ILogger<TransitosController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioTransito servicioTransito
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioTransito = servicioTransito;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        if (id.HasValue)
        {
            var data = await _servicioTransito.ObtenerTransito(id.Value);
            return data != null ? Ok(data) : NoContent();
        }
        else
        {
            var data = await _servicioTransito.ObtenerTransitos();
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
    }

    [HttpPost("Cabecera")]
    public async Task<IActionResult> GuardarCabecera([FromBody] Classes.CabeceraTransito item)
    {
        try
        {
            var result = await _servicioTransito.GuardarCabeceraTransito(item);
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
            var data = await _servicioTransito.Archivar(id);
            return data.Exito ? Ok() : StatusCode(500, data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpPost("Linea")]
    public async Task<IActionResult> GuardarLinea([FromBody] List<Classes.LineaTransito> items)
    {
        try
        {
            var result = await _servicioTransito.GuardarLineaTransito(items);
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
            var result = await _servicioTransito.EliminarLineaTransito(id);
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
            var result = await _servicioTransito.ObtenerActivas();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }
}
