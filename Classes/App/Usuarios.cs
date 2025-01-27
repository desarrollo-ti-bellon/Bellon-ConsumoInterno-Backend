
// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class Usuarios
{

    [JsonPropertyName("id_usuario")]
    public int? IdUsuario;

    [JsonPropertyName("nombre_usuario")]
    public string NombreUsuario;

    [JsonPropertyName("correo")]
    public string Correo;

    [JsonPropertyName("codigo_sucursal")]
    public string CodigoSucursal;

    [JsonPropertyName("id_sucursal")]
    public string IdSucursal;

    [JsonPropertyName("codigo_departamento")]
    public string CodigoDepartamento;

    [JsonPropertyName("id_departamento")]
    public string IdDepartamento;

    [JsonPropertyName("limite")]
    public decimal Limite;

    [JsonPropertyName("posicion_id")]
    public string PosicionId;

    [JsonPropertyName("estado")]
    public bool Estado;

}