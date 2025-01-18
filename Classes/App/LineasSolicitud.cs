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

    [JsonPropertyName("precio")]
    public decimal Precio { get; set; }

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }

}
