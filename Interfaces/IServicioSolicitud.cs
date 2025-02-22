namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioSolicitud
{
    Task<List<Classes.CabeceraSolicitudCI>> ObtenerSolicitudesPorPerfilUsuario();

    Task<List<Classes.CabeceraSolicitudCI>> ObtenerSolicitudesDelUsuarioSolicitantePorEstado(int? estadoSolicitudId);

    Task<List<Classes.CabeceraSolicitudCI>> ObtenerSolicitudes();

    Task<Classes.CabeceraSolicitudCI> ObtenerSolicitudesPorId(int id);

    Task<List<Classes.CabeceraSolicitudCI>> ObtenerSolicitudesPorEstadoSolicitud(int? estadoSolicitudId);

    Task<int> ObtenerCantidadSolicitudesPorEstadoSolicitud(int estadoSolicitudId);

    Task<Classes.CabeceraSolicitudCI> ObtenerSolicitud(int id);

    Task<Classes.CabeceraSolicitudCI> GuardarSolicitud(Classes.CabeceraSolicitudCI item);

    Task<Classes.CabeceraSolicitudCI> GuardarLineasSolicitud(List<Classes.LineasSolicitudCI> productos);

    Task<List<Classes.CabeceraSolicitudCI>> RecuperarSolicitud(int id);

    Task<Classes.CabeceraSolicitudCI> EliminarSolicitud(int id);

    Task<Classes.CabeceraSolicitudCI> EliminarLineaSolicitud(int id);
   
    Task<Classes.Resultado> Archivar(int id);

    Task<bool> RefrescarCache();
}
