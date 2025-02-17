namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioEstadoSolicitud
{
    Task<List<Classes.EstadoSolicitudCI>> ObtenerEstadoSolicitudes();
    Task<List<Classes.EstadoSolicitudCI>> ObtenerEstadoSolicitud(int id);
    Task<List<Classes.EstadoSolicitudCI>> GuardarEstadoSolicitud(Classes.EstadoSolicitudCI item);
    Task<List<Classes.EstadoSolicitudCI>> EliminarEstadoSolicitud(int id);
    Task<bool> RefrescarCache();
}
