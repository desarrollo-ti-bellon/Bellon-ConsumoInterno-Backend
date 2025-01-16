namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioHistLlegada
{
    Task<List<Classes.HistCabeceraLlegada>> ObtenerLlegadas();

    Task<Classes.HistCabeceraLlegada> ObtenerLlegada(int id);

    Task<Classes.Resultado> Recuperar(int id);

    Task<bool> RefrescarCache(bool incluyeProduccion = false);
}
