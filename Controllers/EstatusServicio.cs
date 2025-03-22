using Azure;
using Bellon.API.ConsumoInterno.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace Bellon.API.ConsumoInterno.Controllers;

[ApiController]
[Route("[controller]")]
public class EstatusServicioController : ControllerBase
{
    private readonly ILogger<EstatusServicioController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;

    public EstatusServicioController(
        ILogger<EstatusServicioController> logger,
        IHttpContextAccessor contextAccessor
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener()
    {
        return Ok();
    }
}
