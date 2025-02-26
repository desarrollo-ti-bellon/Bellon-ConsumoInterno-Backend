namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioImpresionConsumoInterno
{
    Task<List<Classes.ImpresionConsumoInterno>> ObtenerImpresionConsumosInternos();
    Task<bool> RefrescarCache();

}