using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralCodigoArancelario
{
    [JsonPropertyName("@odata.context")]
    public Uri odataContext { get; set; }

    [JsonPropertyName("@odata.etag")]
    public string odataEtag { get; set; }

    [JsonPropertyName("id_cod_arancelario")]
    public Guid IdCodArancelario { get; set; }

    [JsonPropertyName("no")]
    public string No { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }
}
