using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralExistenciaProducto

{

    [JsonPropertyName("codigo_almacen")]
    public string CodigoAlmacen { get; set; }

    [JsonPropertyName("no_producto")]
    public string NoProducto { get; set; }

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }

    [JsonPropertyName("nombre_almacen")]
    public string NombreAlmacen { get; set; }

}
