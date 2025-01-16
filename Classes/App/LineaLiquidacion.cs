// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public partial class LineaLiquidacion
{
    [JsonPropertyName("id_linea_liquidacion")]
    public int? IdLineaLiquidacion { get; set; }

    [JsonPropertyName("cabecera_liquidacion_id")]
    public int CabeceraLiquidacionId { get; set; }

    [JsonPropertyName("hist_cabecera_transito_id")]
    public int HistCabeceraTransitoId { get; set; }

    [JsonPropertyName("hist_cabecera_llegada_id")]
    public int? HistCabeceraLlegadaId { get; set; }

    [JsonPropertyName("id_hist_linea_llegada")]
    public int? IdHistLineaLlegada { get; set; }

    [JsonPropertyName("id_producto")]
    public string IdProducto { get; set; } = null!;

    [JsonPropertyName("no_producto")]
    public string NoProducto { get; set; } = null!;

    [JsonPropertyName("descripcion_producto")]
    public string DescripcionProducto { get; set; } = null!;

    [JsonPropertyName("referencia_producto")]
    public string ReferenciaProducto { get; set; } = null!;

    [JsonPropertyName("codigo_arancelario_id")]
    public int CodigoArancelarioId { get; set; }

    [JsonPropertyName("codigo_arancelario_cod")]
    public string? CodigoArancelarioCod { get; set; } = null;

    [JsonPropertyName("codigo_pais_origen")]
    public string CodigoPaisOrigen { get; set; } = null!;

    [JsonPropertyName("id_unidad_medida")]
    public string IdUnidadMedida { get; set; } = null!;

    [JsonPropertyName("codigo_unidad_medida")]
    public string CodigoUnidadMedida { get; set; } = null!;

    [JsonPropertyName("cantidad")]
    public decimal Cantidad { get; set; }

    [JsonPropertyName("fob_aduana_us")]
    public decimal FobAduanaUs { get; set; }

    [JsonPropertyName("total_cif")]
    public decimal TotalCif { get; set; }

    [JsonPropertyName("total_gravamen")]
    public decimal TotalGravamen { get; set; }

    [JsonPropertyName("total_selectivo")]
    public decimal TotalSelectivo { get; set; }

    [JsonPropertyName("tasa_itbis")]
    public decimal TasaItbis { get; set; }

    [JsonPropertyName("total_itbis")]
    public decimal TotalItbis { get; set; }

    [JsonPropertyName("total_general")]
    public decimal TotalGeneral { get; set; }

    [JsonPropertyName("almacen_id")]
    public string AlmacenId { get; set; } = null!;

    [JsonPropertyName("almacen_codigo")]
    public string AlmacenCodigo { get; set; } = null!;

    [JsonPropertyName("costo_producto_rd")]
    public decimal CostoProductoRd { get; set; }

    [JsonPropertyName("costo_producto_us")]
    public decimal CostoProductoUs { get; set; }

    [JsonPropertyName("tasa_liquidacion")]
    public decimal TasaLiquidacion { get; set; }

    [JsonPropertyName("ultimo_costo_producto_rd")]
    public decimal UltimoCostoProductoRd { get; set; }

    [JsonPropertyName("importe_us")]
    public decimal ImporteUs { get; set; }

    [JsonPropertyName("id_pais_origen")]
    public string IdPaisOrigen { get; set; } = null!;

    [JsonPropertyName("fecha_creado")]
    public DateTime? FechaCreado { get; set; }

    [JsonPropertyName("creado_por")]
    public string? CreadoPor { get; set; } = null!;

    [JsonPropertyName("fecha_modificado")]
    public DateTime? FechaModificado { get; set; }

    [JsonPropertyName("modificado_por")]
    public string? ModificadoPor { get; set; }
}
