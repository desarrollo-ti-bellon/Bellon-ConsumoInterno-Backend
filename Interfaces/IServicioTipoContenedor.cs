namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioTipoContenedor
{
    Task<List<Classes.TipoContenedor>> ObtenerTipoContenedores();

    Task<List<Classes.TipoContenedor>> ObtenerTipoContenedoresActivos();

    Task<Classes.TipoContenedor> ObtenerTipoContenedor(int id);

    Task<Classes.TipoContenedor> GuardarTipoContenedor(Classes.TipoContenedor item);
}
