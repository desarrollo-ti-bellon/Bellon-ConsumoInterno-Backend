using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralAjusteInventarioResultadoArray
{
    [JsonPropertyName("no_producto")]
    public string NoProducto { get; set; }
   
    [JsonPropertyName("exito")]
    public bool Exito { get; set; }
    
    [JsonPropertyName("mensaje")]
    public string Mensaje { get; set; }
}
