namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioSolicitud
{
    Task<List<Classes.CabeceraSolicitud>> ObtenerSolicitudes();
    Task<List<Classes.CabeceraSolicitud>> ObtenerSolicitudesPorId(int id);
    Task<List<Classes.CabeceraSolicitud>> ObtenerSolicitudesPorEstadoSolicitud(int estadoSolicitudId);
    Task<int> ObtenerCantidadSolicitudesPorEstadoSolicitud(int estadoSolicitudId);

    Task<Classes.CabeceraSolicitud> ObtenerSolicitud(int id);

    Task<List<Classes.CabeceraSolicitud>> GuardarSolicitud(Classes.CabeceraSolicitud item);

    Task<List<Classes.CabeceraSolicitud>> GuardarLineasSolicitud(List<Classes.LineasSolicitud> productos);

    Task<List<Classes.CabeceraSolicitud>> RecuperarSolicitud(int id);

    Task<List<Classes.CabeceraSolicitud>> EliminarSolicitud(int id);

    Task<List<Classes.CabeceraSolicitud>> EliminarLineaSolicitud(int id);

    Task<bool> RefrescarCache();
}
