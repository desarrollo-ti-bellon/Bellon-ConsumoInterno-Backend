namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioSucursal
{
    Task<List<Classes.LSCentralSucursal>> ObtenerSucursales();

    Task<Classes.LSCentralSucursal> ObtenerSucursal(string id);

    Task<bool> RefrescarCache();
}
