using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class LSCentralUsuario
{
    [JsonPropertyName("@odata.context")]
    public Uri odataContext { get; set; }

    [JsonPropertyName("@odata.etag")]
    public string odataEtag { get; set; }

    [JsonPropertyName("id_usuario")]
    public Guid IdUsuario { get; set; }

    [JsonPropertyName("nombre_usuario")]
    public string NombreUsuario { get; set; }

    [JsonPropertyName("nombre_completo")]
    public string NombreCompleto { get; set; }

    [JsonPropertyName("estado")]
    public string Estado { get; set; }

    [JsonPropertyName("correo_electronico")]
    public string CorreoElectronico { get; set; }
}
