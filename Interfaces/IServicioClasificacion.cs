namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioClasificacion
{
    Task<List<Classes.LSCentralClasificacion>> ObtenerClasificacionesERP();
    Task<Classes.LSCentralClasificacion> ObtenerClasificacionERP(string id);
    Task<List<Classes.Clasificacion>> ObtenerClasificaciones();
    Task<Classes.Clasificacion> ObtenerClasificacion(int? id);
    Task<Classes.Clasificacion> GuardarClasificacion(Classes.Clasificacion item);
    Task<Classes.Clasificacion> EliminarClasificacion(int id);
    Task<bool> RefrescarCache();
}
