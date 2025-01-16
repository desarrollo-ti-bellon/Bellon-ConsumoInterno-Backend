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
public class UsuariosController : ControllerBase
{
    private readonly ILogger<UsuariosController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioUsuario _servicioUsuario;

    public UsuariosController(
        ILogger<UsuariosController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioUsuario servicioUsuario
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioUsuario = servicioUsuario;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] string? id)
    {
        try
        {
            if (!string.IsNullOrEmpty(id))
            {
                var data = await _servicioUsuario.ObtenerUsuario(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioUsuario.ObtenerUsuarios();
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }
}
