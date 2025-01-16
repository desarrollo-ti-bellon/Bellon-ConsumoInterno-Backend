
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public class LSCentralCargoFactura {
    
    [JsonPropertyName("id_factura")]
    public Guid? IdFactura { get; set; }
    
    [JsonPropertyName("no_factura")]
    public string? NoFactura { get; set; }
    
    [JsonPropertyName("fecha")]
    public DateOnly Fecha { get; set; }
    
    [JsonPropertyName("codigo_almacen")]
    public string? CodigoAlmacen { get; set; }
    
    [JsonPropertyName("no_proveedor")]
    public string? NoProveedor { get; set; }
    
    [JsonPropertyName("nombre_proveedor")]
    public string? NombreProveedor { get; set; }
    
    [JsonPropertyName("no_cargo_producto")]
    public string? NoCargoProducto { get; set; }
    
    [JsonPropertyName("referencia_cargo_producto")]
    public string? ReferenciaCargoProducto { get; set; }
    
    [JsonPropertyName("importe")]
    public double Importe { get; set; }
    
    [JsonPropertyName("importe_sin_iva")]
    public double ImporteSinIva { get; set; }

    [JsonPropertyName("id_linea")]
    public string IdLinea { get; set; }
    
}
