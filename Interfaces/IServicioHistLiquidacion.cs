namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioHistLiquidacion
{
    Task<List<Classes.HistCabeceraLiquidacion>> ObtenerLiquidaciones();

    Task<Classes.HistCabeceraLiquidacion> ObtenerLiquidacion(int id);

    Task<Classes.Resultado> Recuperar(int id);

    Task<bool> RefrescarCache(bool incluyeProduccion = false);
}
