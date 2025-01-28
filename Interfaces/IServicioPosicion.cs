using Bellon.API.Liquidacion.Classes;

namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioPosicion
{
    Task<List<Posicion>> ObtenerPosiciones();
    Task<Posicion> ObtenerPosicion(int? id);
}
