// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class CodigoArancelario
{
    [JsonPropertyName("id_codigo_arancelario")]
    public int? IdCodigoArancelario { get; set; }

    [JsonPropertyName("id_cod_arancelario")]
    public string IdCodArancelario { get; set; }

    [JsonPropertyName("no_cod_arancelario")]
    public string NoCodArancelario { get; set; } = null!;

    [JsonPropertyName("descripcion_cod_arancelario")]
    public string Descripcion { get; set; } = null!;

    [JsonPropertyName("id_pais")]
    public string IdPais { get; set; } = null!;

    [JsonPropertyName("codigo_pais")]
    public string CodigoPais { get; set; } = null!;

    [JsonPropertyName("porciento_selectivo")]
    public decimal PorcientoSelectivo { get; set; }

    [JsonPropertyName("porciento_gravamen")]
    public decimal PorcientoGravamen { get; set; }
}
