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
public class DepartamentoController : ControllerBase
{
    private readonly ILogger<DepartamentoController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioDepartamento _servicioDepartamento;

    public DepartamentoController(
        ILogger<DepartamentoController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioDepartamento servicioDepartamento
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioDepartamento = servicioDepartamento;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] string? id)
    {
        try
        {
            if (!string.IsNullOrEmpty(id))
            {
                var data = await _servicioDepartamento.ObtenerDepartamento(id);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                var data = await _servicioDepartamento.ObtenerDepartamentos();
                return data != null && data.Count > 0 ? Ok(data) : NoContent();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }
}
