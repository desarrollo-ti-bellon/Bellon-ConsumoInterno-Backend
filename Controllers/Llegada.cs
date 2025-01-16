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
public class LlegadasController : ControllerBase
{
    private readonly ILogger<LlegadasController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioLlegada _servicioLlegada;

    public LlegadasController(
        ILogger<LlegadasController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioLlegada servicioLlegada
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioLlegada = servicioLlegada;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        if (id.HasValue)
        {
            var data = await _servicioLlegada.ObtenerLlegada(id.Value);
            return data != null ? Ok(data) : NoContent();
        }
        else
        {
            var data = await _servicioLlegada.ObtenerLlegadas();
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
    }

    [HttpPost("Cabecera")]
    public async Task<IActionResult> GuardarCabecera([FromBody] Classes.CabeceraLlegada item)
    {
        try
        {
            var result = await _servicioLlegada.GuardarCabeceraLlegada(item);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Archivar([FromQuery] int id)
    {
        try
        {
            var data = await _servicioLlegada.Archivar(id);
            return data.Exito ? Ok() : StatusCode(500, data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpPost("Linea")]
    public async Task<IActionResult> GuardarLinea([FromBody] List<Classes.LineaLlegada> items)
    {
        try
        {
            var result = await _servicioLlegada.GuardarLineaLlegada(items);
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
            var result = await _servicioLlegada.EliminarLineaLlegada(id);
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
            var result = await _servicioLlegada.ObtenerActivas();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }
}
