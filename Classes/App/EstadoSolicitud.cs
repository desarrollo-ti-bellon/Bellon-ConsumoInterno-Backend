// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class EstadoSolicitudCI
{
    [JsonPropertyName("id_estado_solicitud")]
    public int? IdEstadoSolicitud { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }

    [JsonPropertyName("estado")]
    public bool Estado { get; set; }

}
