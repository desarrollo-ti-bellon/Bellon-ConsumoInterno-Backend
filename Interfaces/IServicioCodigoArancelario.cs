namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioCodigoArancelario
{
    Task<List<Classes.CodigoArancelario>> ObtenerCodigoArancelarios();

    Task<Classes.CodigoArancelario> ObtenerCodigoArancelario(int id);

    Task<Classes.CodigoArancelario> ObtenerPorcentajesCodigoArancelario(string id);

    Task<Classes.CodigoArancelario> GuardarCodigoArancelario(Classes.CodigoArancelario item);

    Task<List<Classes.LSCentralCodigoArancelario>> ObtenerCodigosArancelarioERP();

    Task<Classes.LSCentralCodigoArancelario> ObtenerCodigoArancelarioERP(string id);

    Task<bool> RefrescarCache();
}
