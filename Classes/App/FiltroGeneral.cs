using System.Text.Json.Serialization;

public class FiltroGeneral
{
    // [JsonPropertyName("no_documento")]
    public string? NoDocumento { get; set; }
    
    // [JsonPropertyName("creado_por")]
    public string? CreadoPor { get; set; }
    
    // [JsonPropertyName("fecha_desde")]
    public DateTime? FechaDesde { get; set; }
    
    // [JsonPropertyName("fecha_hasta")]
    public DateTime? FechaHasta { get; set; }
    
    // [JsonPropertyName("usuario_responsable")]
    public string? UsuarioResponsable { get; set; }
    
    // [JsonPropertyName("estado_solicitud")]
    public int? EstadoSolicitudId { get; set; }
    
    // [JsonPropertyName("id_sucursal")]
    public string? IdSucursal { get; set; }
    
    // [JsonPropertyName("id_departamento")]
    public string? IdDepartamento { get; set; }

}