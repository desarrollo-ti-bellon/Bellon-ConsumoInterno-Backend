using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralAjusteInventarioArray
{
    [JsonPropertyName("@odata.context")]
    public Uri odataContext { get; set; }

    [JsonPropertyName("value")]
    public LSCentralAjusteInventario[] value { get; set; }
}
