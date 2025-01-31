namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioAutorizacion
{
    Task<bool> ValidarUsuarioPerfil(string userName);
    Task<Classes.BusinessCentralToken> ObtenerTokenBC();
}
