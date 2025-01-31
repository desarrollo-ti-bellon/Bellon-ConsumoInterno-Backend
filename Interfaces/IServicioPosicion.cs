using Bellon.API.ConsumoInterno.Classes;

namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioPosicion
{
    Task<List<Posicion>> ObtenerPosiciones();
    Task<Posicion> ObtenerPosicion(int? id);
}
