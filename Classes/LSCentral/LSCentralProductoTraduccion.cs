using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralProductoTraduccion
{
    [JsonPropertyName("@odata.context")]
    public Uri odataContext { get; set; }

    [JsonPropertyName("@odata.etag")]
    public string odataEtag { get; set; }

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("no_producto")]
    public string NoProducto { get; set; }

    [JsonPropertyName("languageCode")]
    public string LanguageCode { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }
}
