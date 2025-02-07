// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class Posicion
{
    [JsonPropertyName("posicion_id")]
    public int? PosicionId { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }

    [JsonPropertyName("crear_solicitud")]
    public bool? CrearSolicitud { get; set; }

    [JsonPropertyName("enviar_solicitud")]
    public bool? EnviarSolicitud { get; set; }

    [JsonPropertyName("registrar_solicitud")]
    public bool? RegistrarSolicitud { get; set; }

    [JsonPropertyName("aprobar_rechazar_solicitud")]
    public bool? AprobarRechazarSolicitud { get; set; }

    [JsonPropertyName("ver_solicitudes")]
    public bool? VerSolicitudes { get; set; }
}
