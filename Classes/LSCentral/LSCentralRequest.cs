using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralRequest
{
    [JsonPropertyName("requestJson")]
    public string RequestJson { get; set; }

}
