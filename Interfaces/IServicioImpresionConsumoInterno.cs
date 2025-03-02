namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioImpresionConsumoInterno
{
    Task<List<Classes.ImpresionConsumoInterno>> ObtenerImpresionConsumosInternos();
    
    Task<List<Classes.ImpresionConsumoInterno>> ObtenerImpresionConsumosInternosConFiltros(FiltroGeneral filtro);
    
    Task<bool> RefrescarCache();

}