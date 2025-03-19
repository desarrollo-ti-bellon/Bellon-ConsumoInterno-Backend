using Bellon.API.ConsumoInterno.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Bellon.API.ConsumoInterno.Services;

public class ServicioDatosCache(IMemoryCache memoryCache) : IServicioDatosCache
{

    private readonly IMemoryCache _memoryCache = memoryCache;

    public Task<bool> BorrarCacheUsuarios()
    {
        _memoryCache.Remove("UsuarioPerfil");
        return Task.FromResult(true);
    }

}