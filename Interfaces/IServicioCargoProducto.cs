namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioCargoProducto
{
    Task<List<Classes.LSCentralCargoProducto>> ObtenerCargosProducto();

    Task<Classes.LSCentralCargoProducto> ObtenerCargoProducto(string id);

    Task<bool> RefrescarCache();
}
