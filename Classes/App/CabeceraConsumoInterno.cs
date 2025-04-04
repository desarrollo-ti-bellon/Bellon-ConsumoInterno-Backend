// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;
using Bellon.API.ConsumoInterno.DataBase;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class CabeceraConsumoInterno
{

    [JsonPropertyName("id_cabecera_consumo_interno")]
    public int? IdCabeceraConsumoInterno { get; set; }

    [JsonPropertyName("no_serie_id")]
    public int? NoSerieId { get; set; }

    [JsonPropertyName("no_documento")]
    public string? NoDocumento { get; set; }

    [JsonPropertyName("fecha_creado")]
    public DateTime FechaCreado { get; set; }

    [JsonPropertyName("creado_por")]
    public string CreadoPor { get; set; }

    [JsonPropertyName("usuario_responsable")]
    public string UsuarioResponsable { get; set; }

    [JsonPropertyName("usuario_despacho")]
    public string UsuarioDespacho { get; set; }

    [JsonPropertyName("id_departamento")]
    public string IdDepartamento { get; set; }

    [JsonPropertyName("id_estado_solicitud")]
    public int IdEstadoSolicitud { get; set; }

    [JsonPropertyName("id_clasificacion")]
    public int IdClasificacion { get; set; }

    [JsonPropertyName("id_sucursal")]
    public string IdSucursal { get; set; }

    [JsonPropertyName("fecha_modificado")]
    public DateTime? FechaModificado { get; set; }

    [JsonPropertyName("modificado_por")]
    public string ModificadoPor { get; set; }

    [JsonPropertyName("comentario")]
    public string Comentario { get; set; }

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("id_usuario_responsable")]
    public int IdUsuarioResponsable { get; set; }

    [JsonPropertyName("id_usuario_despacho")]
    public int? IdUsuarioDespacho { get; set; }

    [JsonPropertyName("lineas")]
    public List<LineasConsumoInterno>? Lineas { get; set; } = new List<LineasConsumoInterno>();

    [JsonPropertyName("cantidad_lineas")]
    public int? CantidadLineas { get; set; }

    [JsonPropertyName("nombre_creado_por")]
    public string? NombreCreadoPor { get; set; }

}
