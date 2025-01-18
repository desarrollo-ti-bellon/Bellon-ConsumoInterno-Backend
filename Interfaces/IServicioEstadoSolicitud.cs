namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioEstadoSolicitud
{
    Task<List<Classes.EstadoSolicitud>> ObtenerEstadoSolicitudes();
    Task<List<Classes.EstadoSolicitud>> ObtenerEstadoSolicitud(int id);
    Task<List<Classes.EstadoSolicitud>> GuardarEstadoSolicitud(Classes.EstadoSolicitud item);
    Task<List<Classes.EstadoSolicitud>> EliminarEstadoSolicitud(int id);
    Task<bool> RefrescarCache();
}
