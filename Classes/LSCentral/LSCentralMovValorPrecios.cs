
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]

public class LSCentralMovValorPrecios
{

    [JsonPropertyName("@odata.etag")]
    public string? @data { get; set; }

    [JsonPropertyName("entryNo")]
    public int? entryNo { get; set; }

    [JsonPropertyName("id")]
    public Guid? id { get; set; }

    [JsonPropertyName("no_documento")]
    public string? no_documento { get; set; }

    [JsonPropertyName("no_producto")]
    public string? noProducto { get; set; }

    [JsonPropertyName("cantidad")]
    public double cantidad { get; set; }

    [JsonPropertyName("no_registro_producto")]
    public int noRegistroProducto { get; set; }

    [JsonPropertyName("precio_neto")]
    public double precioNeto { get; set; }

    [JsonPropertyName("total_neto")]
    public double totalNeto { get; set; }

    [JsonPropertyName("factor_divisa")]
    public double factorDivisa { get; set; }

}