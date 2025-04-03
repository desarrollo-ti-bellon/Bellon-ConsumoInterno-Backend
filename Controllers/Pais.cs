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
public class PaisesController : ControllerBase
{
    private readonly ILogger<PaisesController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioPais _servicioPais;

    public PaisesController(
        ILogger<PaisesController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioPais servicioPais
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioPais = servicioPais;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] string? id)
    {
        try
        {
            if (!string.IsNullOrEmpty(id))
            {
                var data = await _servicioPais.ObtenerPais(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioPais.ObtenerPaises();
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (InvalidDataException ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}
