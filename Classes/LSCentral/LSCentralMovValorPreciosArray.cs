
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]

public class LSCentralMovValorPreciosArray
{

    [JsonPropertyName("@odata.context")]
    public Uri? odataContext { get; set; }

    [JsonPropertyName("value")]
    public LSCentralMovValorPrecios[]? value { get; set; }

}