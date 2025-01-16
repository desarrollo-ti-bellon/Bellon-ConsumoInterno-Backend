namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioLiquidacion
{
    Task<List<Classes.CabeceraLiquidacion>> ObtenerLiquidaciones();

    Task<Classes.CabeceraLiquidacion> ObtenerLiquidacion(int id);

    Task<Classes.CabeceraLiquidacion> GuardarCabeceraLiquidacion(Classes.CabeceraLiquidacion item);

    Task<Classes.CabeceraLiquidacion> GuardarLineaLiquidacion(List<Classes.LineaLiquidacion> items);

    Task<Classes.CabeceraLiquidacion> GuardarCargoAdicionalLiquidacion(
        List<Classes.CargoAdicional> items
    );

    Task<Classes.CabeceraLiquidacion> EliminarLineaLiquidacion(int idTransito);

    Task<Classes.CabeceraLiquidacion> EliminarCargoAdicionalLiquidacion(int item);

    Task<int> ObtenerActivas();

    Task<Classes.Resultado> Archivar(int id);

    Task<List<Classes.LineaLiquidacion>> ObtenerLineasLiquidacion(List<int> transitos);

    Task<Classes.Resultado> CrearTransferencias(int id);

    Task<bool> RefrescarCache(bool incluyeHistorico = false);
}
