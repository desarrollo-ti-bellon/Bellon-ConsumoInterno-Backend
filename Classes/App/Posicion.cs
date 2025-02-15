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

    [JsonPropertyName("aprobar_solicitud")]
    public bool? AprobarSolicitud { get; set; }

    [JsonPropertyName("rechazar_solicitud")]
    public bool? RechazarSolicitud { get; set; }

    [JsonPropertyName("confirmar_solicitud")]
    public bool? ConfirmarSolicitud { get; set; }

    [JsonPropertyName("terminar_solicitud")]
    public bool? TerminarSolicitud { get; set; }

    [JsonPropertyName("entregar_solicitud")]
    public bool? EntregarSolicitud { get; set; }

    
}
