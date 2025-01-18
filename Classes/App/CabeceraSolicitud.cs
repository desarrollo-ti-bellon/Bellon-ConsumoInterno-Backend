// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class CabeceraSolicitud
{

    [JsonPropertyName("id_solicitud")]
    public int? IdSolicitud { get; set; }

    [JsonPropertyName("fecha_creado")]
    public DateTime FechaCreado { get; set; }

    [JsonPropertyName("comentario")]
    public string? Comentario { get; set; }

    [JsonPropertyName("creado_por")]
    public string CreadoPor { get; set; }

    [JsonPropertyName("modificado_por")]
    public string? ModificadoPor { get; set; }

    [JsonPropertyName("fecha_modificado")]
    public DateTime? FechaModificado { get; set; }

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("usuario_aprobador")]
    public string? UsuarioAprobador { get; set; }

    [JsonPropertyName("id_departamento")]
    public string IdDepartamento { get; set; }

    [JsonPropertyName("usuario_despachador")]
    public string? UsuarioDespachador { get; set; }

    [JsonPropertyName("usuario_asistente_control")]
    public string? UsuarioAsistenteControl { get; set; }

    [JsonPropertyName("usuario_asistente_contabilidad")]
    public string? UsuarioAsistenteContabilidad { get; set; }

    [JsonPropertyName("estado_solicitud_id")]
    public int EstadoSolicitudId { get; set; }

    [JsonPropertyName("lineas")]
    public List<LineasSolicitud>? Lineas { get; set; }

}
