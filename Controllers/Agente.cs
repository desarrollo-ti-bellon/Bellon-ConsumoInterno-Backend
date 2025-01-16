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
public class AgentesController : ControllerBase
{
    private readonly ILogger<AgentesController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioAgente _servicioAgente;

    public AgentesController(
        ILogger<AgentesController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioAgente servicioAgente
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioAgente = servicioAgente;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? id)
    {
        if (id.HasValue)
        {
            var data = await _servicioAgente.ObtenerAgente(id.Value);
            return data != null ? Ok(data) : NoContent();
        }
        else
        {
            var data = await _servicioAgente.ObtenerAgentes();
            return data != null && data.Count > 0 ? Ok(data) : NoContent();
        }
    }

    [HttpGet("Activos")]
    public async Task<IActionResult> ObtenerActivos()
    {
        var data = await _servicioAgente.ObtenerAgentesActivos();
        return data != null && data.Count > 0 ? Ok(data) : NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Guardar([FromBody] Classes.Agente item)
    {
        var result = await _servicioAgente.GuardarAgente(item);
        return result != null ? Ok(result) : NoContent();
    }

    [HttpGet("Test")]
    public async Task<IActionResult> Test([FromQuery] int duracion)
    {
        System.Threading.Thread.Sleep(duracion);
        var data = new
        {
            exito = true,
            mensaje = "",
            result = new object[]
            {
                new
                {
                    itemId = Guid.NewGuid().ToString(),
                    itemNo = "ITEM001",
                    itemNombre = "Nombre del item",
                    listaPrecioId = Guid.NewGuid().ToString(),
                    listaPrecioNo = "Codigo de la lista de precio",
                    listaPrecioNombre = "Nombre de la lista",
                    oldPrice = 95.75,
                    newPrice = 105.75,
                    exito = true,
                    mensaje = "",
                },
                new
                {
                    itemId = Guid.NewGuid().ToString(),
                    itemNo = "ITEM001",
                    itemNombre = "Nombre del item",
                    listaPrecioId = Guid.NewGuid().ToString(),
                    listaPrecioNo = "Codigo de la lista de precio",
                    listaPrecioNombre = "Nombre de la lista",
                    oldPrice = 95.75,
                    newPrice = 105.75,
                    exito = false,
                    mensaje = "Mensaje de error",
                },
                new
                {
                    itemId = Guid.NewGuid().ToString(),
                    itemNo = "ITEM001",
                    itemNombre = "Nombre del item",
                    listaPrecioId = Guid.NewGuid().ToString(),
                    listaPrecioNo = "Codigo de la lista de precio",
                    listaPrecioNombre = "Nombre de la lista",
                    oldPrice = 95.75,
                    newPrice = 105.75,
                    exito = true,
                },
                new
                {
                    itemId = Guid.NewGuid().ToString(),
                    itemNo = "ITEM001",
                    itemNombre = "Nombre del item",
                    listaPrecioId = Guid.NewGuid().ToString(),
                    listaPrecioNo = "Codigo de la lista de precio",
                    listaPrecioNombre = "Nombre de la lista",
                    oldPrice = 95.75,
                    newPrice = 105.75,
                    exito = true,
                },
                new
                {
                    itemId = Guid.NewGuid().ToString(),
                    itemNo = "ITEM001",
                    itemNombre = "Nombre del item",
                    listaPrecioId = Guid.NewGuid().ToString(),
                    listaPrecioNo = "Codigo de la lista de precio",
                    listaPrecioNombre = "Nombre de la lista",
                    oldPrice = 95.75,
                    newPrice = 105.75,
                    exito = true,
                },
                new
                {
                    itemId = Guid.NewGuid().ToString(),
                    itemNo = "ITEM001",
                    itemNombre = "Nombre del item",
                    listaPrecioId = Guid.NewGuid().ToString(),
                    listaPrecioNo = "Codigo de la lista de precio",
                    listaPrecioNombre = "Nombre de la lista",
                    oldPrice = 95.75,
                    newPrice = 105.75,
                    exito = true,
                },
                new
                {
                    itemId = Guid.NewGuid().ToString(),
                    itemNo = "ITEM001",
                    itemNombre = "Nombre del item",
                    listaPrecioId = Guid.NewGuid().ToString(),
                    listaPrecioNo = "Codigo de la lista de precio",
                    listaPrecioNombre = "Nombre de la lista",
                    oldPrice = 95.75,
                    newPrice = 105.75,
                    exito = true,
                },
                new
                {
                    itemId = Guid.NewGuid().ToString(),
                    itemNo = "ITEM001",
                    itemNombre = "Nombre del item",
                    listaPrecioId = Guid.NewGuid().ToString(),
                    listaPrecioNo = "Codigo de la lista de precio",
                    listaPrecioNombre = "Nombre de la lista",
                    oldPrice = 95.75,
                    newPrice = 105.75,
                    exito = true,
                },
                new
                {
                    itemId = Guid.NewGuid().ToString(),
                    itemNo = "ITEM001",
                    itemNombre = "Nombre del item",
                    listaPrecioId = Guid.NewGuid().ToString(),
                    listaPrecioNo = "Codigo de la lista de precio",
                    listaPrecioNombre = "Nombre de la lista",
                    oldPrice = 95.75,
                    newPrice = 105.75,
                    exito = true,
                },
                new
                {
                    itemId = Guid.NewGuid().ToString(),
                    itemNo = "ITEM001",
                    itemNombre = "Nombre del item",
                    listaPrecioId = Guid.NewGuid().ToString(),
                    listaPrecioNo = "Codigo de la lista de precio",
                    listaPrecioNombre = "Nombre de la lista",
                    oldPrice = 95.75,
                    newPrice = 105.75,
                    exito = true,
                },
            },
        };
        return Ok(data);
    }
}
