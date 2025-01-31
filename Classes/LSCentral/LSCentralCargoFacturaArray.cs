using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralCargoFacturaArray
{
    [JsonPropertyName("@odata.context")]
    public Uri? odataContext { get; set; }

    [JsonPropertyName("value")]
    public LSCentralCargoFactura[]? value { get; set; }
}
