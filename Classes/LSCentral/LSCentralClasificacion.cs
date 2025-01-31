
using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]

public class LSCentralClasificacion
{

    [JsonPropertyName("@odata.etag")]
    public string? @data { get; set; }

    [JsonPropertyName("codigo")]
    public string? Codigo { get; set; }

    [JsonPropertyName("id_grupo_cont_producto_general")]
    public Guid? IdGrupoContProductoGeneral { get; set; }

    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }

    [JsonPropertyName("id_codigo_grupo_iva_producto_nobd")]
    public Guid? IdCodigoGrupoIvaProductoNobd { get; set; }

    [JsonPropertyName("codigo_grupo_iva_producto")]
    public string? CodigoGrupoIvaProducto { get; set; }

}