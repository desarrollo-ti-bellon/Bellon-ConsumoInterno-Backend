
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]

public class LSCentralClasificacionArray
{

    [JsonPropertyName("@odata.context")]
    public Uri? odataContext { get; set; }

    [JsonPropertyName("value")]
    public LSCentralClasificacion[]? value { get; set; }

}