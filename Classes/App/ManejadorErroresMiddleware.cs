using System.Text.Json;
using Bellon.API.ConsumoInterno.Classes;

public class ManejadorErroresMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ManejadorErroresMiddleware> _logger;

    // Inicializa una nueva instancia de la clase.
    public ManejadorErroresMiddleware(RequestDelegate next, ILogger<ManejadorErroresMiddleware> logger)
    {
        _next = next;
        _logger = logger; //El logger para registrar errores.
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Registra la excepción con un mensaje de error.
            _logger.LogError(ex, "Se produjo una excepción no controlada.");
            // Maneja la excepción y genera una respuesta adecuada.
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status200OK;

        var stackTrace = exception?.StackTrace?.Split(["\r\n"], StringSplitOptions.None)[0]?.TrimStart()?.ToUpper();
         var response = new PeticionRespuesta
        {
            Exito = false,
            Mensaje = $"{exception?.Message} {stackTrace}."
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}