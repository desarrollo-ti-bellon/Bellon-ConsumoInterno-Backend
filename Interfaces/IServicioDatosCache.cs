namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioDatosCache
{
    Task<bool> BorrarCacheUsuarios();
}