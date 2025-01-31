using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralAlmacen
{
    [JsonPropertyName("@odata.context")]
    public Uri odataContext { get; set; }

    [JsonPropertyName("@odata.etag")]
    public string odataEtag { get; set; }

    [JsonPropertyName("id_almacen")]
    public Guid IdAlmacen { get; set; }

    [JsonPropertyName("codigo")]
    public string Codigo { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; }

    [JsonPropertyName("direccion")]
    public string Direccion { get; set; }

    [JsonPropertyName("codigo_postal")]
    public string CodigoPostal { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("contact")]
    public string Contact { get; set; }

    [JsonPropertyName("phoneNo")]
    public string PhoneNo { get; set; }
}
