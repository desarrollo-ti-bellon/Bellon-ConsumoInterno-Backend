namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioLlegada
{
    Task<List<Classes.CabeceraLlegada>> ObtenerLlegadas();

    Task<Classes.CabeceraLlegada> ObtenerLlegada(int id);

    Task<Classes.CabeceraLlegada> GuardarCabeceraLlegada(Classes.CabeceraLlegada item);

    Task<Classes.CabeceraLlegada> GuardarLineaLlegada(List<Classes.LineaLlegada> items);

    Task<Classes.CabeceraLlegada> EliminarLineaLlegada(int item);

    void CalcularTotalCabecera(int item);

    Task<int> ObtenerActivas();

    Task<Classes.Resultado> Archivar(int id);

    Task<bool> RefrescarCache(bool incluyeHistorico = false);
}
