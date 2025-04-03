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
public class ProductosController : ControllerBase
{
    private readonly ILogger<ProductosController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioProducto _servicioProducto;

    public ProductosController(
        ILogger<ProductosController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioProducto servicioProducto
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioProducto = servicioProducto;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] string? id)
    {
        try
        {
            if (!string.IsNullOrEmpty(id))
            {
                var data = await _servicioProducto.ObtenerProducto(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioProducto.ObtenerProductos();
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

    [HttpGet("DisponibilidadProducto")]
    public async Task<IActionResult> ObtenerExistenciaProductosAlmacenes([FromQuery] string codigoAlmacen)
    {
        try
        {
            if (!string.IsNullOrEmpty(codigoAlmacen))
            {
                var data = await _servicioProducto.DisponibilidadProducto(codigoAlmacen);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                throw new Exception($"debes indicar un codigo almacen");
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

    [HttpPost("ObtenerTraducciones")]
    public async Task<IActionResult> ObtenerProductosPorIds([FromBody] List<Guid> ids)
    {
        try
        {
            if (ids != null && ids.Count > 0)
            {
                var data = await _servicioProducto.ObtenerTraduccionesProductosPorIds(ids);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                return BadRequest(new Classes.Resultado { Exito = false, Mensaje = "No se han proporcionado los identificadores de los productos" });
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
