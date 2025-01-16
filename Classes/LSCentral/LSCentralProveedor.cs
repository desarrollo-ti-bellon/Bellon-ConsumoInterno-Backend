using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class LSCentralProveedor
{
    [JsonPropertyName("@odata.context")]
    public Uri odataContext { get; set; }

    [JsonPropertyName("@odata.etag")]
    public string odataEtag { get; set; }

    [JsonPropertyName("id_proveedor")]
    public Guid IdProveedor { get; set; }

    [JsonPropertyName("codigo")]
    public string Codigo { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; }

    [JsonPropertyName("direccion")]
    public string Direccion { get; set; }

    [JsonPropertyName("codigo_pais")]
    public string CodigoPais { get; set; }

    [JsonPropertyName("no_telefono")]
    public string NoTelefono { get; set; }

    [JsonPropertyName("correo_electronico")]
    public string CorreoElectronico { get; set; }

    [JsonPropertyName("contacto")]
    public string Contacto { get; set; }

    [JsonPropertyName("balance")]
    public double Balance { get; set; }

    [JsonPropertyName("codigo_divisa")]
    public string CodigoDivisa { get; set; }

    [JsonPropertyName("grupo_registro_proveedor")]
    public string GrupoRegistroProveedor { get; set; }
}
