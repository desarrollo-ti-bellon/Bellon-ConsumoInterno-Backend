
using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]

public class LSCentralSucursalArray
{

    [JsonPropertyName("@odata.context")]
    public Uri? odataContext { get; set; }

    [JsonPropertyName("value")]
    public LSCentralSucursal[]? value { get; set; }

}