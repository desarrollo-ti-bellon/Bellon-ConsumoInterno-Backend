// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public partial class CabeceraTransito
{
    [JsonPropertyName("id_cabecera_transito")]
    public int? IdCabeceraTransito { get; set; }

    [JsonPropertyName("no_serie_id")]
    public int? NoSerieId { get; set; }

    [JsonPropertyName("no_documento")]
    public string? NoDocumento { get; set; } = null!;

    [JsonPropertyName("fecha_documento")]
    public DateOnly FechaDocumento { get; set; }

    [JsonPropertyName("fecha_registro")]
    public DateOnly FechaRegistro { get; set; }

    [JsonPropertyName("nombre_proveedor")]
    public string NombreProveedor { get; set; } = null!;

    [JsonPropertyName("naviera")]
    public string Naviera { get; set; } = null!;

    [JsonPropertyName("no_conocimiento_embarque")]
    public string NoConocimientoEmbarque { get; set; } = null!;

    [JsonPropertyName("no_sello")]
    public string NoSello { get; set; } = null!;

    [JsonPropertyName("no_buque")]
    public string NoBuque { get; set; } = null!;

    [JsonPropertyName("tipo_contenedor_id")]
    public int TipoContenedor_id { get; set; }

    [JsonPropertyName("no_contenedor")]
    public string NoContenedor { get; set; } = null!;

    [JsonPropertyName("fecha_embarque")]
    public DateOnly FechaEmbarque { get; set; }

    [JsonPropertyName("puerto_embarque")]
    public string PuertoEmbarque { get; set; } = null!;

    [JsonPropertyName("fecha_desembarque")]
    public DateOnly FechaDesembarque { get; set; }

    [JsonPropertyName("puerto_desembarque")]
    public string PuertoDesembarque { get; set; } = null!;

    [JsonPropertyName("almacen_id")]
    public string AlmacenId { get; set; } = null!;

    [JsonPropertyName("almacen_codigo")]
    public string AlmacenCodigo { get; set; } = null!;

    [JsonPropertyName("fecha_estimada")]
    public DateOnly FechaEstimada { get; set; }

    [JsonPropertyName("detalle_mercancia")]
    public string DetalleMercancia { get; set; } = null!;

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

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
    public List<LineaTransito>? Lineas { get; set; }
}
