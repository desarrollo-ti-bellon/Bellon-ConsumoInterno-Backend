// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public partial class OrdenTransferencia
{
    [JsonPropertyName("id_orden_transferencia")]
    public string IdOrdenTransferencia { get; set; }

    [JsonPropertyName("no_orden_transferencia")]
    public string NoOrdenTransferencia { get; set; }

    [JsonPropertyName("hist_cabecera_transito_id")]
    public int HistCabeceraTransitoId { get; set; }

    [JsonPropertyName("hist_cabecera_liquidacion_id")]
    public int HistCabeceraLiquidacionId { get; set; }

    [JsonPropertyName("id_orden_envio")]
    public string IdOrdenEnvio { get; set; } = null!;

    [JsonPropertyName("no_orden_envio")]
    public string NoOrdenEnvio { get; set; } = null!;

    [JsonPropertyName("fecha_creado")]
    public DateTime FechaCreado { get; set; }

    [JsonPropertyName("creado_por")]
    public string CreadoPor { get; set; } = null!;

    [JsonPropertyName("fecha_modificado")]
    public DateTime? FechaModificado { get; set; }

    [JsonPropertyName("modificado_por")]
    public string? ModificadoPor { get; set; }
}
