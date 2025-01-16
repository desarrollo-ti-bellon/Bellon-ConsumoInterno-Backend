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
public class DocumentosOrigenController : ControllerBase
{
    private readonly ILogger<DocumentosOrigenController> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Interfaces.IServicioDocumentoOrigen _servicioDocumentoOrigen;

    public DocumentosOrigenController(
        ILogger<DocumentosOrigenController> logger,
        IHttpContextAccessor contextAccessor,
        Interfaces.IServicioDocumentoOrigen servicioDocumentoOrigen
    )
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _servicioDocumentoOrigen = servicioDocumentoOrigen;
    }

    [HttpGet("Recepciones")]
    public async Task<IActionResult> ObtenerRecepciones([FromQuery] string id)
    {
        try
        {
            var data = await _servicioDocumentoOrigen.ObtenerRecepciones(id);
            return data != null ? Ok(data) : NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }

    [HttpGet("Facturas")]
    public async Task<IActionResult> ObtenerFactura(
        [FromQuery] string id,
        [FromQuery] string noRecepcion
    )
    {
        try
        {
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(noRecepcion))
            {
                var data = await _servicioDocumentoOrigen.ObtenerFactura(id, noRecepcion);
                return data != null ? Ok(data) : NoContent();
            }
            else
            {
                return StatusCode(
                    500,
                    new Classes.Resultado
                    {
                        Exito = false,
                        Mensaje = "Los pará,etros id y noRecepcion son obligatorios",
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new Classes.Resultado { Exito = false, Mensaje = ex.Message });
        }
    }
}
