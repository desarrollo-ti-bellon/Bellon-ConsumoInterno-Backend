using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralAjusteInventario
{
    [JsonPropertyName("no_documento")]
    public string NoDocumento { get; set; }

    [JsonPropertyName("nombre_diario")]
    public string NombreDiario { get; set; }

    [JsonPropertyName("nombre_seccion_diario")]
    public string NombreSeccionDiario { get; set; }

    [JsonPropertyName("no_linea")]
    public int NoLinea { get; set; }

    [JsonPropertyName("tipo_movimiento")]
    public string TipoMovimiento { get; set; }

    [JsonPropertyName("no_producto")]
    public string NoProducto { get; set; }

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }

    [JsonPropertyName("codigo_almacen")]
    public string CodigoAlmacen { get; set; }

}