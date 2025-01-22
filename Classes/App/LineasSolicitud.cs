// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class LineasSolicitud
{

    [JsonPropertyName("id_linea_solicitud")]
    public int? IdLineaSolicitud { get; set; }

    [JsonPropertyName("cabecera_solicitud_id")]
    public int CabeceraSolicitudId { get; set; }

    [JsonPropertyName("id_producto")]
    public Guid IdProducto { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }

    [JsonPropertyName("precio_unitario")]
    public decimal PrecioUnitario { get; set; }

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }

    [JsonPropertyName("id_unidad_medida")]
    public Guid IdUnidadMedida { get; set; }

    [JsonPropertyName("codigo_unidad_medida")]
    public string CodigoUnidadMedida { get; set; }

    [JsonPropertyName("almacen_id")]
    public Guid? AlmacenId { get; set; }

    [JsonPropertyName("almacen_codigo")]
    public string? AlmacenCodigo { get; set; }

    [JsonPropertyName("nota")]
    public string? Nota { get; set; }

}
