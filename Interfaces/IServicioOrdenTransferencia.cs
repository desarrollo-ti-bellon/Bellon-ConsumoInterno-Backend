namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioOrdenTransferencia
{
    Task<List<Classes.OrdenTransferencia>> ObtenerOrdenes();

    Task<List<Classes.OrdenTransferencia>> ObtenerOrdenes(int idLiquidacion);

    Task<bool> RefrescarCache();

    Task<Classes.Resultado> CrearTransferencias(int idLiquidacion);
}
