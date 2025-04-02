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
public class PosicionesController : ControllerBase
{
    private readonly ILogger<PosicionesController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioPosicion _servicioPosicion;

    public PosicionesController(
        ILogger<PosicionesController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioPosicion ServicioPosicion
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioPosicion = ServicioPosicion;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        try
        {
            if (id != null)
            {
                var data = await _servicioPosicion.ObtenerPosicion(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioPosicion.ObtenerPosiciones();
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (Exception ex)
        {
            throw ex;
            // return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

}
