using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralAjusteInventarioValor
{
    [JsonPropertyName("exito")]
    public bool Exito { get; set; }

    [JsonPropertyName("mensaje")]
    public string Mensaje { get; set; }

    [JsonPropertyName("result")]
    public List<LSCentralAjusteInventarioResultadoArray>? Result { get; set; }
}
