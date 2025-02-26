// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;
using Bellon.API.ConsumoInterno.DataBase;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class ImpresionConsumoInterno
{

    [JsonPropertyName("id_producto")]
    public string IdProducto { get; set; }

    [JsonPropertyName("no_producto")]
    public string NoProducto { get; set; }

    [JsonPropertyName("fecha_creado")]
    public DateTime FechaCreado { get; set; }

    [JsonPropertyName("no_documento")]
    public string NoDocumento { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }

    [JsonPropertyName("id_clasificacion")]
    public int IdClasificacion { get; set; }

    [JsonPropertyName("clasificacion_descripcion")]
    public string ClasificacionDescripcion { get; set; }

    [JsonPropertyName("almacen_id")]
    public string AlmacenId { get; set; }

    [JsonPropertyName("almacen_codigo")]
    public string AlmacenCodigo { get; set; }

    [JsonPropertyName("cantidad_total")]
    public int? CantidadTotal { get; set; }

    [JsonPropertyName("precio_unitario_total")]
    public decimal? PrecioUnitarioTotal { get; set; }

    [JsonPropertyName("total")]
    public decimal? Total { get; set; }

}
