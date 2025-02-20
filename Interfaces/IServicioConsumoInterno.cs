namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioConsumoInterno
{
    Task<List<Classes.CabeceraConsumoInterno>> ObtenerConsumosInternosDelUsuarioSolicitantePorEstado(int? estadoConsumoInternoId);

    Task<List<Classes.CabeceraConsumoInterno>> ObtenerConsumosInternos();

    Task<Classes.CabeceraConsumoInterno> ObtenerConsumoInternoPorId(int id);

    Task<List<Classes.CabeceraConsumoInterno>> ObtenerConsumoInternoPorEstadoSolicitud(int? estadoConsumoInternoId);

    Task<int> ObtenerCantidadConsumoInternosPorEstadoSolicitud(int estadoConsumoInternoId);

    Task<Classes.CabeceraConsumoInterno> ObtenerConsumoInterno(int id);

    Task<bool> RefrescarCache();
}
