namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioClasificacion
{
    Task<List<Classes.LSCentralClasificacion>> ObtenerClasificacionesERP();
    Task<Classes.LSCentralClasificacion> ObtenerClasificacionERP(string id);
    Task<List<Classes.ClasificacionCI>> ObtenerClasificaciones();
    Task<Classes.ClasificacionCI> ObtenerClasificacion(int? id);
    Task<Classes.ClasificacionCI> GuardarClasificacion(Classes.ClasificacionCI item);
    Task<Classes.ClasificacionCI> EliminarClasificacion(int id);
    Task<bool> RefrescarCache();
}
