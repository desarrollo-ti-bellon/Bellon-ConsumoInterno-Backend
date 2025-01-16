
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Bellon.API.Liquidacion.Authorization;

namespace Bellon.API.Liquidacion.Controllers;

[Authorize]
[AutorizacionBellon]
[RequiredScope("App.Liquidacion")]
[ApiController]
[Route("[controller]")]
public class MovValorPreciosController : ControllerBase
{
    private readonly ILogger<MovValorPreciosController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioMovValorPrecios _servicioMovValorPrecios;

    public MovValorPreciosController(
        ILogger<MovValorPreciosController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioMovValorPrecios servicioMovValorPrecios
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioMovValorPrecios = servicioMovValorPrecios;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] string? id)
    {
        if (id != null)
        {
            var data = await _servicioMovValorPrecios.ObtenerMovValorPrecios(id);
            return data != null ? Ok(data) : NoContent();
        }
        else
        {
            return NoContent();
        }
    }

}