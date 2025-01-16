using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class CargoFacturaLiquidacion
{

    [JsonPropertyName("id_cargo_factura_liquidacion")]
    public int? IdCargoFacturaLiquidacion { get; set; }

    [JsonPropertyName("cabecera_liquidacion_id")]
    public int CabeceraLiquidacionId { get; set; }

    [JsonPropertyName("no_documento_liquidacion")]
    public string NoDocumentoLiquidacion { get; set; } = null!;

    [JsonPropertyName("id_factura")]
    public string IdFactura { get; set; } = null!;

    [JsonPropertyName("no_factura")]
    public string NoFactura { get; set; } = null!;

    [JsonPropertyName("fecha")]
    public DateTime Fecha { get; set; }

    [JsonPropertyName("importe")]
    public decimal Importe { get; set; }

    [JsonPropertyName("importe_sin_iva")]
    public decimal ImporteSinIVA { get; set; }

    [JsonPropertyName("fecha_creado")]
    public DateTime FechaCreado { get; set; }

    [JsonPropertyName("creado_por")]
    public string? CreadoPor { get; set; } = null!;

    [JsonPropertyName("fecha_modificado")]
    public DateTime? FechaModificado { get; set; }

    [JsonPropertyName("modificado_por")]
    public string? ModificadoPor { get; set; }

    [JsonPropertyName("no_cargo_producto")]
    public string NoCargoProducto { get; set; }

    [JsonPropertyName("referencia_cargo_producto")]
    public string? ReferenciaCargoProducto { get; set; }

    [JsonPropertyName("no_proveedor")]
    public string NoProveedor { get; set; }

    [JsonPropertyName("nombre_proveedor")]
    public string NombreProveedor { get; set; }

}