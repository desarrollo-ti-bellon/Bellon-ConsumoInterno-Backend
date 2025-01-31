
using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]

public class LSCentralSucursal
{

    [JsonPropertyName("@odata.etag")]
    public string? @data { get; set; }

    [JsonPropertyName("codigo_dimension")]
    public string? CodigoDimension { get; set; }

    [JsonPropertyName("codigo")]
    public string Codigo { get; set; }

    [JsonPropertyName("id_valor_dimension")]
    public Guid? IdValorDimension { get; set; }

    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }

    [JsonPropertyName("id_dimension_nobd")]
    public Guid? IdDimensionNobd { get; set; }

}