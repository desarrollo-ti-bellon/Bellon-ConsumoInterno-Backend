
// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class Clasificacion
{
    [JsonPropertyName("id_clasificacion")]
    public int Id_clasificacion;

    [JsonPropertyName("id_grupo_cont_producto_general")]
    public string IdGrupoContProductoGeneral;

    [JsonPropertyName("codigo_clasificacion")]
    public string CodigoClasificacion;

    [JsonPropertyName("descripcion")]
    public string Descripcion;

    [JsonPropertyName("estado")]
    public bool Estado;
}