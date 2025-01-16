using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class LSCentralDocumentoOrigen
{
    [JsonPropertyName("@odata.context")]
    public Uri odataContext { get; set; }

    [JsonPropertyName("@odata.etag")]
    public string odataEtag { get; set; }

    [JsonPropertyName("id_documento_origen")]
    public Guid IdDocumentoOrigen { get; set; }

    [JsonPropertyName("no_documento_origen")]
    public string NoDocumentoOrigen { get; set; }

    [JsonPropertyName("fecha_documento_origen")]
    public DateTime FechaDocumentoOrigen { get; set; }

    [JsonPropertyName("no_documento_previo")]
    public string NoDocumentoPrevio { get; set; }

    [JsonPropertyName("codigo_almacen")]
    public string CodigoAlmacen { get; set; }

    [JsonPropertyName("factor_divisa")]
    public decimal FactorDivisa { get; set; }

    [JsonPropertyName("no_proveedor")]
    public string NoProveedor { get; set; }

    [JsonPropertyName("nombre_proveedor")]
    public string NombreProveedor { get; set; }

    [JsonPropertyName("no_factura_proveedor")]
    public string NoFacturaProveedor { get; set; }

    [JsonPropertyName("codigo_divisa")]
    public string CodigoDivisa { get; set; }

    [JsonPropertyName("no_lineas")]
    public int NoLineas { get; set; }

    [JsonPropertyName("almacenes")]
    public LSCentralAlmacen[] Almacenes { get; set; }

    [JsonPropertyName("proveedores")]
    public LSCentralProveedor[] Proveedores { get; set; }

    [JsonPropertyName("lineaOrdCompras")]
    public LSCentralDocumentoOrigenLinea[] LineaOrdCompras { get; set; }

    [JsonPropertyName("lineaFacCompras")]
    public LSCentralDocumentoOrigenLinea[] LineaFacCompras { get; set; }
}
