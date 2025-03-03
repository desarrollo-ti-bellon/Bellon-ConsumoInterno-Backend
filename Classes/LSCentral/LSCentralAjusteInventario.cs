using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralAjusteInventario
{
    [JsonPropertyName("fecha_registro")]
    public DateOnly FechaRegistro { get; set; }

    [JsonPropertyName("fecha_documento")]
    public DateOnly FechaDocumento { get; set; }

    [JsonPropertyName("no_orden")]
    public string NoOrden { get; set; }

    [JsonPropertyName("nombre_diario")]
    public string NombreDiario { get; set; }

    [JsonPropertyName("nombre_seccion_diario")]
    public string NombreSeccionDiario { get; set; }

    [JsonPropertyName("codigo_auditoria")]
    public string CodigoAuditoria { get; set; }

    [JsonPropertyName("no_linea")]
    public int NoLinea { get; set; }

    [JsonPropertyName("codigo_almacen")]
    public string CodigoAlmacen { get; set; }

    [JsonPropertyName("tipo_movimiento")]
    public string TipoMovimiento { get; set; }

    [JsonPropertyName("no_producto")]
    public string NoProducto { get; set; }

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }

}
