using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralExistenciaProductoArray
{

    [JsonPropertyName("@odata.context")]
    public Uri odataContext { get; set; }

    [JsonPropertyName("value")]
    public LSCentralExistenciaProducto[] value { get; set; }

}
