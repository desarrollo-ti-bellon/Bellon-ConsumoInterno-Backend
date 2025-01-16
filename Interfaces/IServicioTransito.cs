namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioTransito
{
    Task<List<Classes.CabeceraTransito>> ObtenerTransitos();

    Task<Classes.CabeceraTransito> ObtenerTransito(int id);

    Task<Classes.CabeceraTransito> GuardarCabeceraTransito(Classes.CabeceraTransito item);

    Task<Classes.CabeceraTransito> GuardarLineaTransito(List<Classes.LineaTransito> items);

    Task<Classes.CabeceraTransito> EliminarLineaTransito(int item);

    void CalcularTotalCabecera(int item);

    Task<Classes.Resultado> Archivar(int id);

    Task<int> ObtenerActivas();

    Task<bool> RefrescarCache(bool incluyeHistorico = false);
}
