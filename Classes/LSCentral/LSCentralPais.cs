using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralPais
{
    [JsonPropertyName("@odata.context")]
    public Uri odataContext { get; set; }

    [JsonPropertyName("@odata.etag")]
    public string odataEtag { get; set; }

    [JsonPropertyName("id_pais")]
    public Guid IdPais { get; set; }

    [JsonPropertyName("codigo")]
    public string Codigo { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; }
}
