// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class Posicion
{
    [JsonPropertyName("posicion_id")]
    public int? PosicionId { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }

    [JsonPropertyName("limite_maximo_permitido")]
    public decimal LimiteMaximoPermitido { get; set; }

}
