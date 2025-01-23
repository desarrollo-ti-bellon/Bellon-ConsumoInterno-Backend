namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioSolicitud
{

    Task<List<Classes.CabeceraSolicitud>> ObtenerSolicitudesDelUsuarioSolicitantePorEstado(int? estadoSolicitudId);
    Task<List<Classes.CabeceraSolicitud>> ObtenerSolicitudes();
    Task<Classes.CabeceraSolicitud> ObtenerSolicitudesPorId(int id);
    Task<List<Classes.CabeceraSolicitud>> ObtenerSolicitudesPorEstadoSolicitud(int? estadoSolicitudId);
    Task<int> ObtenerCantidadSolicitudesPorEstadoSolicitud(int estadoSolicitudId);

    Task<Classes.CabeceraSolicitud> ObtenerSolicitud(int id);

    Task<Classes.CabeceraSolicitud> GuardarSolicitud(Classes.CabeceraSolicitud item);

    Task<Classes.CabeceraSolicitud> GuardarLineasSolicitud(List<Classes.LineasSolicitud> productos);

    Task<List<Classes.CabeceraSolicitud>> RecuperarSolicitud(int id);

    Task<Classes.CabeceraSolicitud> EliminarSolicitud(int id);

    Task<Classes.CabeceraSolicitud> EliminarLineaSolicitud(int id);

    Task<bool> RefrescarCache();
}
