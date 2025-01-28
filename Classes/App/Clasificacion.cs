
// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class Clasificacion
{
    [JsonPropertyName("id_clasificacion")]
    public int? IdClasificacion { get; set; }

    [JsonPropertyName("id_grupo_cont_producto_general")]
    public string IdGrupoContProductoGeneral { get; set; }

    [JsonPropertyName("codigo_clasificacion")]
    public string CodigoClasificacion { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }

    [JsonPropertyName("estado")]
    public bool Estado { get; set; }
}