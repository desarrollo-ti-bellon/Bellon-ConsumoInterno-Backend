
// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class Usuario
{

    [JsonPropertyName("id_usuario_ci")]
    public int? IdUsuarioCI { get; set; }

    [JsonPropertyName("id_usuario")]
    public string IdUsuario { get; set; }

    [JsonPropertyName("nombre_usuario")]
    public string NombreUsuario { get; set; }

    [JsonPropertyName("correo")]
    public string Correo { get; set; }

    [JsonPropertyName("codigo_sucursal")]
    public string CodigoSucursal { get; set; }

    [JsonPropertyName("id_sucursal")]
    public string IdSucursal { get; set; }

    [JsonPropertyName("codigo_departamento")]
    public string CodigoDepartamento { get; set; }

    [JsonPropertyName("id_departamento")]
    public string IdDepartamento { get; set; }

    [JsonPropertyName("limite")]
    public decimal Limite { get; set; }

    [JsonPropertyName("posicion_id")]
    public int PosicionId { get; set; }

    [JsonPropertyName("estado")]
    public bool Estado { get; set; }

    [JsonPropertyName("id_almacen")]
    public string? IdAlmacen { get; set; }

    [JsonPropertyName("codigo_almacen")]
    public string? CodigoAlmacen { get; set; }

    [JsonPropertyName("posicion")]
    [InverseProperty("Usuario")]
    public virtual Posicion? Posicion { get; set; }

}