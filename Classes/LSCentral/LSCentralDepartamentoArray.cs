
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]

public class LSCentralDepartamentoArray
{

    [JsonPropertyName("@odata.context")]
    public Uri? odataContext { get; set; }

    [JsonPropertyName("value")]
    public LSCentralDepartamento[]? value { get; set; }

}