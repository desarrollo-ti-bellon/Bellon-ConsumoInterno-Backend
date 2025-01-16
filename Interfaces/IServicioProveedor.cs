namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioProveedor
{
    Task<List<Classes.LSCentralProveedor>> ObtenerProveedores();

    Task<Classes.LSCentralProveedor> ObtenerProveedor(string id);

    Task<bool> RefrescarCache();
}
