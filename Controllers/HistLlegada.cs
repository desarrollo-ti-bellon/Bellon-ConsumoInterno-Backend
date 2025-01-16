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
public class HistLlegadasController : ControllerBase
{
    private readonly ILogger<HistLlegadasController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioHistLlegada _servicioHistLlegada;

    public HistLlegadasController(
        ILogger<HistLlegadasController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioHistLlegada servicioHistLlegada
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioHistLlegada = servicioHistLlegada;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        if (id.HasValue)
        {
            var data = await _servicioHistLlegada.ObtenerLlegada(id.Value);
            return data != null ? Ok(data) : NoContent();
        }
        else
        {
            var data = await _servicioHistLlegada.ObtenerLlegadas();
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
    }

    [HttpGet("Recuperar")]
    public async Task<IActionResult> Recuperar([FromQuery] int id)
    {
        var data = await _servicioHistLlegada.Recuperar(id);
        return data.Exito ? Ok() : StatusCode(500, data);
    }
}
