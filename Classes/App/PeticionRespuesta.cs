using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class PeticionRespuesta
{
    [JsonPropertyName("exito")]
    public bool Exito { get; set; }

    [JsonPropertyName("mensaje")]
    public string Mensaje { get; set; }
}