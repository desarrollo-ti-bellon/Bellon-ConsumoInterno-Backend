namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioClasificacion
{
    Task<List<Classes.LSCentralClasificacion>> ObtenerClasificaciones();

    Task<Classes.LSCentralClasificacion> ObtenerClasificacion(string id);

    Task<bool> RefrescarCache();
}
