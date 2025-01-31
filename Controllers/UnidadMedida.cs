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
public class UnidadesDeMedidaController : ControllerBase
{
    private readonly ILogger<UnidadesDeMedidaController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioUnidadMedida _servicioUnidadMedida;

    public UnidadesDeMedidaController(
        ILogger<UnidadesDeMedidaController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioUnidadMedida servicioUnidadMedida
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioUnidadMedida = servicioUnidadMedida;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] string? id, [FromQuery] string? filtro)
    {
        try
        {
            if (!string.IsNullOrEmpty(id))
            {
                var data = await _servicioUnidadMedida.ObtenerUnidadMedida(id);
                return data != null ? Ok(data) : NoContent();
            }
            else if (!string.IsNullOrEmpty(filtro))
            {
                var data = await _servicioUnidadMedida.ObtenerUnidadesMedida(filtro);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioUnidadMedida.ObtenerUnidadesMedida(string.Empty);
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }
}
