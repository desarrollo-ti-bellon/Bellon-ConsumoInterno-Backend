// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class Notas
{
    [JsonPropertyName("id_nota")]
    public int? IdNota { get; set; }

    [JsonPropertyName("id_documento")]
    public int IdDocumento { get; set; }

    [JsonPropertyName("tipo_documento")]
    public string TipoDocumento { get; set; }

    [JsonPropertyName("no_documento")]
    public string NoDocumento { get; set; }

    [JsonPropertyName("usuario_destino")]
    public string UsuarioDestino { get; set; } = null!;

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }

    [JsonPropertyName("fecha_creado")]
    public DateTime FechaCreado { get; set; }

    [JsonPropertyName("creado_por")]
    public string CreadoPor { get; set; }

    [JsonPropertyName("fecha_modificado")]
    public DateTime? FechaModificado { get; set; } = null!;

    [JsonPropertyName("modificado_por")]
    public string? ModificadoPor { get; set; } = null!;
}