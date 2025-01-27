// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class Posicion
{
    [JsonPropertyName("posicion_id")]
    public int? PosicionId;

    [JsonPropertyName("descripcion")]
    public string Descripcion;

    [JsonPropertyName("limite_maximo_permitido")]
    public decimal LimiteMaximoPermitido;

}
