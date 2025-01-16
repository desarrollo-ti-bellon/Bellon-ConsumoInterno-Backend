using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class LSCentralProducto
{
    [JsonPropertyName("@odata.context")]
    public Uri odataContext { get; set; }

    [JsonPropertyName("@odata.etag")]
    public string odataEtag { get; set; }

    [JsonPropertyName("id_producto")]
    public Guid IdProducto { get; set; }

    [JsonPropertyName("no")]
    public string No { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }

    [JsonPropertyName("codigo_unidad_medida")]
    public string CodigoUnidadMedida { get; set; }

    [JsonPropertyName("precio_unitario")]
    public double PrecioUnitario { get; set; }

    [JsonPropertyName("costo_unitario")]
    public double CostoUnitario { get; set; }

    [JsonPropertyName("no_cod_arancelario")]
    public string NoCodArancelario { get; set; }

    [JsonPropertyName("inventario")]
    public double Inventario { get; set; }

    [JsonPropertyName("codigo_pais_origen")]
    public string CodigoPaisOrigen { get; set; }

    [JsonPropertyName("codigo_categoria_producto")]
    public string CodigoCategoriaProducto { get; set; }

    [JsonPropertyName("traduccionesProducto")]
    public LSCentralProductoTraduccion[] TraduccionesProducto { get; set; }

    [JsonPropertyName("unidadesDeMedidas")]
    public LSCentralUnidadMedida[] UnidadesDeMedidas { get; set; }

    [JsonPropertyName("codArancelarios")]
    public LSCentralCodigoArancelario[] CodArancelarios { get; set; }

    [JsonPropertyName("paises")]
    public LSCentralPais[] Paises { get; set; }

    [JsonPropertyName("categoriasProducto")]
    public LSCentralCategoriaProducto[] CategoriasProducto { get; set; }
}
