using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class LSCentralCargoProducto
{
    [JsonPropertyName("@odata.context")]
    public Uri odataContext { get; set; }

    [JsonPropertyName("@odata.etag")]
    public string odataEtag { get; set; }

    [JsonPropertyName("id_cargo_producto")]
    public Guid IdCargoProducto { get; set; }

    [JsonPropertyName("no")]
    public string No { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }

    [JsonPropertyName("Grupo_contable_producto")]
    public string GrupoContableProducto { get; set; }

    [JsonPropertyName("grupo_itbis_producto")]
    public string GrupoItbisProducto { get; set; }
}
