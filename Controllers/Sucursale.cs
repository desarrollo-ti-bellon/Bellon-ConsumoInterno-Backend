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
public class SucursalController : ControllerBase
{
    private readonly ILogger<SucursalController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioSucursal _servicioSucursal;

    public SucursalController(
        ILogger<SucursalController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioSucursal servicioSucursal
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioSucursal = servicioSucursal;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] string? id)
    {
        try
        {
            if (!string.IsNullOrEmpty(id))
            {
                var data = await _servicioSucursal.ObtenerSucursal(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioSucursal.ObtenerSucursales();
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }
}
