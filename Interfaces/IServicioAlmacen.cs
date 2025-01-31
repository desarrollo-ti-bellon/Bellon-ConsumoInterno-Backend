using Bellon.API.ConsumoInterno.Classes;

namespace Bellon.API.ConsumoInterno.Interfaces;
public interface IServicioAlmacen
{
    Task<List<LSCentralAlmacen>> ObtenerAlmacenes();

    Task<LSCentralAlmacen> ObtenerAlmacen(string id);

    Task<bool> RefrescarCache();
}
