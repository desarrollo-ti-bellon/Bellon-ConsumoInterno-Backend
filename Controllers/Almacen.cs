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
public class AlmacenesController : ControllerBase
{
    private readonly ILogger<AlmacenesController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioAlmacen _servicioAlmacen;

    public AlmacenesController(
        ILogger<AlmacenesController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioAlmacen servicioAlmacen
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioAlmacen = servicioAlmacen;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] string? id)
    {
        try
        {
            if (!string.IsNullOrEmpty(id))
            {
                var data = await _servicioAlmacen.ObtenerAlmacen(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioAlmacen.ObtenerAlmacenes();
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
