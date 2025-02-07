// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class CabeceraSolicitud
{

    [JsonPropertyName("id_cabecera_solicitud")]
    public int? IdCabeceraSolicitud { get; set; }

    [JsonPropertyName("no_documento")]
    public string? NoDocumento { get; set; }

    [JsonPropertyName("fecha_creado")]
    public DateTime FechaCreado { get; set; }

    [JsonPropertyName("comentario")]
    public string Comentario { get; set; }

    [JsonPropertyName("creado_por")]
    public string CreadoPor { get; set; }

    [JsonPropertyName("usuario_responsable")]
    public string UsuarioResponsable { get; set; }

    [JsonPropertyName("usuario_despacho")]
    public string? UsuarioDespacho { get; set; }

    [JsonPropertyName("usuario_asistente_control")]
    public string? UsuarioAsistenteControl { get; set; }

    [JsonPropertyName("usuario_asistente_contabilidad")]
    public string? UsuarioAsistenteContabilidad { get; set; }

    [JsonPropertyName("id_departamento")]
    public string? IdDepartamento { get; set; }

    [JsonPropertyName("id_estado_solicitud")]
    public int IdEstadoSolicitud { get; set; }

    [JsonPropertyName("id_clasificacion")]
    public int IdClasificacion { get; set; }

    [JsonPropertyName("id_sucursal")]
    public string IdSucursal { get; set; }

    [JsonPropertyName("fecha_modificado")]
    public DateTime? FechaModificado { get; set; }

    [JsonPropertyName("modificado_por")]
    public string? ModificadoPor { get; set; }

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("id_usuario_responsable")]
    public int IdUsuarioResponsable { get; set; }

    [JsonPropertyName("id_usuario_despacho")]
    public int? IdUsuarioDespacho { get; set; }

    [JsonPropertyName("id_usuario_asistente_inventario")]
    public int?IdUsuarioAsistenteInventario { get; set; }

    [JsonPropertyName("id_usuario_asistente_contabilidad")]
    public int? IdUsuarioAsistenteContabilidad { get; set; }

    [JsonPropertyName("lineas")]
    public List<LineasSolicitud>? Lineas { get; set; }
}
