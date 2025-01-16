// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public partial class HistCabeceraLlegada
{
    [JsonPropertyName("id_hist_cabecera_llegada")]
    public int? IdHistCabeceraLlegada { get; set; }

    [JsonPropertyName("no_serie_id")]
    public int NoSerieId { get; set; }

    [JsonPropertyName("no_documento")]
    public string NoDocumento { get; set; } = null!;

    [JsonPropertyName("fecha_documento")]
    public DateOnly FechaDocumento { get; set; }

    [JsonPropertyName("fecha_registro")]
    public DateOnly FechaRegistro { get; set; }

    [JsonPropertyName("id_documento_origen")]
    public string IdDocumentoOrigen { get; set; } = null!;

    [JsonPropertyName("no_documento_origen")]
    public string NoDocumentoOrigen { get; set; } = null!;

    [JsonPropertyName("no_documento_previo")]
    public string NoDocumentoPrevio { get; set; } = null!;

    [JsonPropertyName("id_proveedor")]
    public string IdProveedor { get; set; } = null!;

    [JsonPropertyName("no_proveedor")]
    public string NoProveedor { get; set; } = null!;

    [JsonPropertyName("nombre_proveedor")]
    public string NombreProveedor { get; set; } = null!;

    [JsonPropertyName("nombre_agente")]
    public string? NombreAgente { get; set; } = null!;

    [JsonPropertyName("no_factura_proveedor")]
    public string NoFacturaProveedor { get; set; } = null!;

    [JsonPropertyName("no_almacen_us")]
    public string NoAlmacenUS { get; set; } = null!;

    [JsonPropertyName("transportista")]
    public string Transportista { get; set; } = null!;

    [JsonPropertyName("cantidad_pieza")]
    public decimal CantidadPieza { get; set; }

    [JsonPropertyName("id_peso")]
    public string IdPeso { get; set; } = null!;

    [JsonPropertyName("codigo_peso")]
    public string CodigoPeso { get; set; } = null!;

    [JsonPropertyName("cantidad_peso")]
    public decimal CantidadPeso { get; set; }

    [JsonPropertyName("id_volumen")]
    public string IdVolumen { get; set; } = null!;

    [JsonPropertyName("codigo_volumen")]
    public string CodigoVolumen { get; set; } = null!;

    [JsonPropertyName("cantidad_volumen")]
    public decimal? CantidadVolumen { get; set; }

    [JsonPropertyName("total")]
    public decimal? Total { get; set; }

    [JsonPropertyName("agente_id")]
    public int AgenteId { get; set; }

    [JsonPropertyName("fecha_creado")]
    public DateTime? FechaCreado { get; set; }

    [JsonPropertyName("creado_por")]
    public string? CreadoPor { get; set; } = null!;

    [JsonPropertyName("fecha_modificado")]
    public DateTime? FechaModificado { get; set; }

    [JsonPropertyName("modificado_por")]
    public string? ModificadoPor { get; set; }

    [JsonPropertyName("cantidad_lineas")]
    public int? CantidadLineas { get; set; }

    [JsonPropertyName("lineas")]
    public List<HistLineaLlegada>? Lineas { get; set; }
}
