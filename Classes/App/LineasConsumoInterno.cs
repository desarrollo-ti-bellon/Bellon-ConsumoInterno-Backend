// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LineasConsumoInterno
{

    [JsonPropertyName("id_linea_consumo_interno")]
    public int? IdLineaConsumoInterno { get; set; }

    [JsonPropertyName("cabecera_consumo_interno_id")]
    public int? CabeceraConsumoInternoId { get; set; }

    [JsonPropertyName("id_producto")]
    public string IdProducto { get; set; }

    [JsonPropertyName("no_producto")]
    public string NoProducto { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }

    [JsonPropertyName("precio_unitario")]
    public decimal PrecioUnitario { get; set; }

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }

    [JsonPropertyName("id_unidad_medida")]
    public string IdUnidadMedida { get; set; }

    [JsonPropertyName("codigo_unidad_medida")]
    public string CodigoUnidadMedida { get; set; }

    [JsonPropertyName("almacen_id")]
    public string? AlmacenId { get; set; }

    [JsonPropertyName("almacen_codigo")]
    public string? AlmacenCodigo { get; set; }

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("nota")]
    public string? Nota { get; set; }

}
