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
public class ProveedoresController : ControllerBase
{
    private readonly ILogger<ProveedoresController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioProveedor _servicioProveedor;

    public ProveedoresController(
        ILogger<ProveedoresController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioProveedor servicioProveedor
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioProveedor = servicioProveedor;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] string? id)
    {
        try
        {
            if (!string.IsNullOrEmpty(id))
            {
                var data = await _servicioProveedor.ObtenerProveedor(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioProveedor.ObtenerProveedores();
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
