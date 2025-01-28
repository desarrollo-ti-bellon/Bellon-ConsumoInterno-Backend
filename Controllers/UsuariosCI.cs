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
public class UsuariosCIController : ControllerBase
{
    private readonly ILogger<UsuariosController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioUsuarioCI _servicioUsuarioCI;

    public UsuariosCIController(
        ILogger<UsuariosController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioUsuarioCI servicioUsuarioCI
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioUsuarioCI = servicioUsuarioCI;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        try
        {
            if (id != null)
            {
                var data = await _servicioUsuarioCI.ObtenerUsuario(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioUsuarioCI.ObtenerUsuarios();
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }

    }

    [HttpPost]
    public async Task<IActionResult> Guardar([FromBody] Classes.Usuario item)
    {
        var result = await _servicioUsuarioCI.GuardarUsuario(item);
        return result != null ? Ok(result) : NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Eliminar([FromQuery] int id)
    {
        try
        {
            var result = await _servicioUsuarioCI.EliminarUsuario(id);
            return result != null ? Ok(result) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

}