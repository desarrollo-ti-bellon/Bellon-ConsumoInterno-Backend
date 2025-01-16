// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public partial class LineaTransito
{
    [JsonPropertyName("id_linea_transito")]
    public int? IdLineaTransito { get; set; }

    [JsonPropertyName("cabecera_transito_id")]
    public int CabeceraTransitoId { get; set; }

    [JsonPropertyName("hist_cabecera_llegada_id")]
    public int HistCabeceraLlegadaId { get; set; }

    [JsonPropertyName("hist_cabecera_llegada")]
    public CabeceraLlegada? HistCabeceraLlegada { get; set; }

    [JsonPropertyName("no_llegada")]
    public string NoLlegada { get; set; } = null!;

    [JsonPropertyName("fecha_creado")]
    public DateTime? FechaCreado { get; set; }

    [JsonPropertyName("creado_por")]
    public string? CreadoPor { get; set; } = null!;

    [JsonPropertyName("fecha_modificado")]
    public DateTime? FechaModificado { get; set; }

    [JsonPropertyName("modificado_por")]
    public string? ModificadoPor { get; set; }
}
