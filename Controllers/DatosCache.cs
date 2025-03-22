using System.Text;
using Bellon.API.ConsumoInterno.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bellon.API.ConsumoInterno.Controllers;

[ApiController]
[Route("[controller]")]
public class DatosCacheController : ControllerBase
{
    private readonly IServicioDatosCache _servicioDatosCache;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public DatosCacheController(
        IServicioDatosCache servicioDatosCache,
        IServicioAutorizacion servicioAutorizacion
    )
    {
        _servicioDatosCache = servicioDatosCache;
        _servicioAutorizacion = servicioAutorizacion;
    }

    [HttpDelete("Usuarios")]
    public async Task<IActionResult> Eliminar([FromHeader] string Authorization)
    {
        if (string.IsNullOrEmpty(Authorization))
        {
            return BadRequest(new { mensaje = "Encabezado de autorización faltante." });
        }
        try
        {
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            var decodedAuth = encoding.GetString(Convert.FromBase64String(Authorization));
            var separatorIndex = decodedAuth.IndexOf(':');
            if (separatorIndex == -1 || separatorIndex == decodedAuth.Length - 1)
            {
                return BadRequest(new { mensaje = "Usuario invalido." });
            }
            var username = decodedAuth[(separatorIndex + 1)..];
            var isAuthorizedUser = await _servicioAutorizacion.ValidarUsuarioPerfilAdminUsuario(
                username
            );
            if (!isAuthorizedUser)
            {
                return Unauthorized(new { mensaje = "Acción no permitida." });
            }

            await _servicioDatosCache.BorrarCacheUsuarios();
            return Ok(new { mensaje = "Acción realizada correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = ex.Message });
        }
    }
}
