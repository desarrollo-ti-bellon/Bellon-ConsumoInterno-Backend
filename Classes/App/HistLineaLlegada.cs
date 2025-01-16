// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public partial class HistLineaLlegada
{
    [JsonPropertyName("id_hist_linea_llegada")]
    public int IdHistLineaLlegada { get; set; }

    [JsonPropertyName("hist_cabecera_llegada_id")]
    public int HistCabeceraLlegadaId { get; set; }

    [JsonPropertyName("id_producto")]
    public string IdProducto { get; set; } = null!;

    [JsonPropertyName("no_producto")]
    public string NoProducto { get; set; } = null!;

    [JsonPropertyName("descripcion_producto")]
    public string DescripcionProducto { get; set; } = null!;

    [JsonPropertyName("referencia_producto")]
    public string ReferenciaProducto { get; set; } = null!;

    [JsonPropertyName("id_unidad_medida")]
    public string IdUnidadMedida { get; set; } = null!;

    [JsonPropertyName("codigo_unidad_medida")]
    public string CodigoUnidadMedida { get; set; } = null!;

    [JsonPropertyName("id_pais")]
    public string IdPais { get; set; } = null!;

    [JsonPropertyName("codigo_pais_origen")]
    public string CodigoPaisOrigen { get; set; } = null!;

    [JsonPropertyName("cantidad_origen")]
    public decimal CantidadOrigen { get; set; }

    [JsonPropertyName("cantidad")]
    public decimal Cantidad { get; set; }

    [JsonPropertyName("precio_unitario")]
    public decimal PrecioUnitario { get; set; }

    [JsonPropertyName("costo_unitario")]
    public decimal CostoUnitario { get; set; }

    [JsonPropertyName("costo_unitario_directo")]
    public decimal CostoUnitarioDirecto { get; set; }

    [JsonPropertyName("almacen_id")]
    public string AlmacenId { get; set; } = null!;

    [JsonPropertyName("almacen_codigo")]
    public string AlmacenCodigo { get; set; } = null!;

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("id_cod_arancelario")]
    public string IdCodArancelario { get; set; } = null!;

    [JsonPropertyName("no_cod_arancelario")]
    public string NoCodArancelario { get; set; } = null!;

    [JsonPropertyName("fecha_creado")]
    public DateTime? FechaCreado { get; set; }

    [JsonPropertyName("creado_por")]
    public string? CreadoPor { get; set; } = null!;

    [JsonPropertyName("fecha_modificado")]
    public DateTime? FechaModificado { get; set; }

    [JsonPropertyName("modificado_por")]
    public string? ModificadoPor { get; set; }
}
