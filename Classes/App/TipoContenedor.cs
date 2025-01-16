// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class TipoContenedor
{
    [JsonPropertyName("id_tipo_contenedor")]
    public int? IdTipoContenedor { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }

    [JsonPropertyName("estado")]
    public bool Estado { get; set; }
}
