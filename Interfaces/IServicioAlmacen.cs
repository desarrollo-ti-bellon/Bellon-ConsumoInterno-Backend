namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioAlmacen
{
    Task<List<Classes.LSCentralAlmacen>> ObtenerAlmacenes();

    Task<Classes.LSCentralAlmacen> ObtenerAlmacen(string id);

    Task<bool> RefrescarCache();
}
