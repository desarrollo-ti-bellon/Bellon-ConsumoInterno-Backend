namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioConsumoInterno
{
    Task<List<Classes.CabeceraConsumoInterno>> ObtenerConsumosInternosSegunPosicionUsuario();
    Task<List<Classes.CabeceraConsumoInterno>> ObtenerConsumosInternosSegunPosicionUsuarioYFiltrosGenerales(FiltroGeneral filtro);
    Task<List<Classes.CabeceraConsumoInterno>> ObtenerConsumosInternos();
    Task<Classes.CabeceraConsumoInterno> ObtenerConsumoInternoPorId(int id);
    Task<int> ObtenerCantidadConsumoInternos();
    Task<Classes.CabeceraConsumoInterno> ObtenerConsumoInterno(int id);
    Task<bool> RefrescarCache();

}
