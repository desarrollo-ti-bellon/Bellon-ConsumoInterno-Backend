using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralTraducciones
{
    [JsonPropertyName("@odata.etag")]
    public Uri @odataEtag { get; set; }

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("no_producto")]
    public string NoProducto { get; set; }

    [JsonPropertyName("codigo_idioma")]
    public string CodigoIdioma { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }
}
