namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioHistorialMovimientosSolicitudes
{
    Task<List<Classes.HistorialMovimientoSolicitudCI>> ObtenerHistorialMovimientosSolicitudes();
    Task<List<Classes.HistorialMovimientoSolicitudCI>> ObtenerHistorialMovimientoSolicitud(int? id);
    Task<List<Classes.HistorialMovimientoSolicitudCI>> GuardarHistorialMovimientoSolicitud(Classes.HistorialMovimientoSolicitudCI item);
    Task<List<Classes.HistorialMovimientoSolicitudCI>> EliminarHistorialMovimientoSolicitud(int id);
    Task<bool> RefrescarCache();
}
