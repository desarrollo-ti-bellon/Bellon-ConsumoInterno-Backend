namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioHistTransito
{
    Task<List<Classes.HistCabeceraTransito>> ObtenerTransitos();

    Task<List<Classes.HistCabeceraTransito>> ObtenerTransitos(int idLiquidacion);

    Task<Classes.HistCabeceraTransito> ObtenerTransito(int id);

    Task<Classes.Resultado> Recuperar(int id);

    Task<bool> RefrescarCache(bool incluyeProduccion = false);
}
