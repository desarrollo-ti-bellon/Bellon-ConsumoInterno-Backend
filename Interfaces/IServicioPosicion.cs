using Bellon.API.ConsumoInterno.Classes;

namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioPosicion
{
    Task<List<PosicionUsuarioCI>> ObtenerPosiciones();
    Task<PosicionUsuarioCI> ObtenerPosicion(int? id);
}
