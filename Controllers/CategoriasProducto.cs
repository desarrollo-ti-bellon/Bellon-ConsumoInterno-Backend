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
public class CategoriasProductoController : ControllerBase
{
    private readonly ILogger<CategoriasProductoController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioCategoriaProducto _servicioCategoriaProducto;

    public CategoriasProductoController(
        ILogger<CategoriasProductoController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioCategoriaProducto servicioCategoriaProducto
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioCategoriaProducto = servicioCategoriaProducto;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] string? id)
    {
        try
        {
            if (!string.IsNullOrEmpty(id))
            {
                var data = await _servicioCategoriaProducto.ObtenerCategoriaProducto(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioCategoriaProducto.ObtenerCategoriasProducto();
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }
}
