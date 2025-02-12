namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioHistorialMovimientosSolicitudes
{
    Task<List<Classes.HistorialMovimientoSolicitud>> ObtenerHistorialMovimientosSolicitudes();
    Task<List<Classes.HistorialMovimientoSolicitud>> ObtenerHistorialMovimientoSolicitud(int? id);
    Task<List<Classes.HistorialMovimientoSolicitud>> GuardarHistorialMovimientoSolicitud(Classes.HistorialMovimientoSolicitud item);
    Task<List<Classes.HistorialMovimientoSolicitud>> EliminarHistorialMovimientoSolicitud(int id);
    Task<bool> RefrescarCache();
}
