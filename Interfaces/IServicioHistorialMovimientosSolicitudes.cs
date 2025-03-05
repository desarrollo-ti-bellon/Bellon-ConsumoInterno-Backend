namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioHistorialMovimientosSolicitudes
{
    Task<List<Classes.HistorialMovimientoSolicitudCI>> ObtenerHistorialMovimientosSolicitudes(string documento);
    Task<List<Classes.HistorialMovimientoSolicitudCI>> ObtenerHistorialMovimientosSolicitudesAgrupados();
    Task<List<Classes.HistorialMovimientoSolicitudCI>> ObtenerHistorialMovimientosSolicitudesAgrupadosConFiltrosGenerales(FiltroGeneral filtro);
    Task<Classes.HistorialMovimientoSolicitudCI> ObtenerHistorialMovimientoSolicitud(int? id);
    Task<List<Classes.HistorialMovimientoSolicitudCI>> ObtenerHistorialMovimientosSolicitudes();
    Task<int> ObtenerCantidadHistorialMovimientoSolicitud();
    Task<bool> RefrescarCache();
}
