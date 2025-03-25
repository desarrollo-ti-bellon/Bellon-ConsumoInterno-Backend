// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class PosicionUsuarioCI
{
    [JsonPropertyName("posicion_id")]
    public int? PosicionId { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }

    [JsonPropertyName("crear_solicitud")]
    public bool? CrearSolicitud { get; set; }

    [JsonPropertyName("enviar_solicitud")]
    public bool? EnviarSolicitud { get; set; }

    [JsonPropertyName("aprobar_solicitud")]
    public bool? AprobarSolicitud { get; set; }

    [JsonPropertyName("rechazar_solicitud")]
    public bool? RechazarSolicitud { get; set; }

    [JsonPropertyName("confirmar_solicitud")]
    public bool? ConfirmarSolicitud { get; set; }

    [JsonPropertyName("entregar_solicitud")]
    public bool? EntregarSolicitud { get; set; }

    [JsonPropertyName("cancelar_solicitud")]
    public bool? CancelarSolicitud { get; set; }

}
