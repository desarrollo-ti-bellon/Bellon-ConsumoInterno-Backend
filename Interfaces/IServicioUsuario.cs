namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioUsuario
{
    Task<List<Classes.LSCentralUsuario>> ObtenerUsuarios();

    Task<Classes.LSCentralUsuario> ObtenerUsuario(string id);

    Task<bool> RefrescarCache();
}
