using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class LSCentralDocumentoOrigenLinea
{
    [JsonPropertyName("@odata.context")]
    public Uri odataContext { get; set; }

    [JsonPropertyName("@odata.etag")]
    public string odataEtag { get; set; }

    [JsonPropertyName("id_linea_documento_origen")]
    public Guid IdLineaDocumentoOrigen { get; set; }

    [JsonPropertyName("no_documento_origen")]
    public string NoDocumentoOrigen { get; set; }

    [JsonPropertyName("no_documento_previo")]
    public string NoDocumentoPrevio { get; set; }

    [JsonPropertyName("no_producto")]
    public string NoProducto { get; set; }

    [JsonPropertyName("descripcion_producto")]
    public string DescripcionProducto { get; set; }

    [JsonPropertyName("referencia_producto")]
    public string ReferenciaProducto { get; set; }

    [JsonPropertyName("codigo_almacen")]
    public string CodigoAlmacen { get; set; }

    [JsonPropertyName("cantidad_origen")]
    public double CantidadOrigen { get; set; }

    [JsonPropertyName("cantidad_pendiente")]
    public double CantidadPendiente { get; set; }

    [JsonPropertyName("precio_unitario")]
    public double PrecioUnitario { get; set; }

    [JsonPropertyName("costo_unitario")]
    public double CostoUnitario { get; set; }

    [JsonPropertyName("costo_unitario_directo")]
    public double CostoUnitarioDirecto { get; set; }

    [JsonPropertyName("codigo_unidad_medida")]
    public string CodigoUnidadMedida { get; set; }

    [JsonPropertyName("monto")]
    public double Monto { get; set; }

    [JsonPropertyName("productos")]
    public LSCentralProducto[] Productos { get; set; }

    [JsonPropertyName("almacenes")]
    public LSCentralAlmacen[] Almacenes { get; set; }

    [JsonPropertyName("unidadesDeMedidas")]
    public LSCentralUnidadMedida[] UnidadesDeMedidas { get; set; }
}
