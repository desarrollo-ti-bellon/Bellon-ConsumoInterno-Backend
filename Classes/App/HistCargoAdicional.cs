// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public partial class HistCargoAdicional
{
    [JsonPropertyName("id_hist_cargo_adicional")]
    public int IdHistCargoAdicional { get; set; }

    [JsonPropertyName("hist_cabecera_liquidacion_id")]
    public int HisCabeceraLiquidacionId { get; set; }

    [JsonPropertyName("id_cargo_producto")]
    public string IdCargoProducto { get; set; } = null!;

    [JsonPropertyName("no_cargo_producto")]
    public string NoCargoProducto { get; set; } = null!;

    [JsonPropertyName("descripcion_cargo_producto")]
    public string DescripcionCargoProducto { get; set; } = null!;

    [JsonPropertyName("fecha_documento")]
    public DateTime FechaDocumento { get; set; }

    [JsonPropertyName("monto_documento")]
    public decimal MontoDocumento { get; set; }

    [JsonPropertyName("observacion")]
    public string Observacion { get; set; } = null!;

    [JsonPropertyName("fecha_creado")]
    public DateTime? FechaCreado { get; set; }

    [JsonPropertyName("creado_por")]
    public string? CreadoPor { get; set; } = null!;

    [JsonPropertyName("fecha_modificado")]
    public DateTime? FechaModificado { get; set; }

    [JsonPropertyName("modificado_por")]
    public string? ModificadoPor { get; set; }
}
