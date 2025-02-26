using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Bellon.API.ConsumoInterno.Classes;
using Bellon.API.ConsumoInterno.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bellon.API.ConsumoInterno.Services;

public class ServicioProducto : IServicioProducto
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioProducto(
        IHttpContextAccessor httpContextAccessor,
        DataBase.AppDataBase context,
        IOptions<AppSettings> settings,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IServicioAutorizacion servicioAutorizacion
    )
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _servicioAutorizacion = servicioAutorizacion;
    }

    public async Task<List<LSCentralExistenciaProducto>> DisponibilidadProducto(string codigoAlmacen)
    {
        var token = await _servicioAutorizacion.ObtenerTokenBC();
        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
        var httpResponseMessage = await httpClient.GetAsync($"QDisponibilidadesProducto?$filter=codigo_almacen eq '{codigoAlmacen}'");

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<Classes.LSCentralExistenciaProductoArray>(contentStream);
            if (data != null)
            {
                return data.value.OrderBy(i => i.NoProducto).ToList();
            }
        }
        else
        {
            var errorMessage = await httpResponseMessage.Content.ReadAsStringAsync();
            throw new Exception($"Error en el servicio de Producto de LS Central: {errorMessage}");
        }

        return null;
    }

    public async Task<List<LSCentralProducto>> ObtenerProductos()
    {
        var cache = _memoryCache.Get<LSCentralProductoArray>("Productos");
        if (cache == null)
        {
            var token = await _servicioAutorizacion.ObtenerTokenBC();
            var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
            var httpResponseMessage = await httpClient.GetAsync("Productos");
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                cache = await JsonSerializer.DeserializeAsync<Classes.LSCentralProductoArray>(
                    contentStream
                );
                if (cache != null)
                {
                    _memoryCache.Set<LSCentralProductoArray>(
                        "Productos",
                        cache,
                        DateTimeOffset.Now.AddMinutes(60)
                    );
                }
            }
            else
            {
                throw new Exception("Error en el servicio de Productos de LS Central");
            }
        }

        return cache
            .value.Where(i => !string.IsNullOrEmpty(i.Descripcion))
            .OrderBy(i => i.Descripcion)
            .ToList();
    }

    public async Task<LSCentralProducto> ObtenerProducto(string id)
    {
        var token = await _servicioAutorizacion.ObtenerTokenBC();
        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
        httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
        var httpResponseMessage = await httpClient.GetAsync(
            "Productos("
                + id
                + ")?$expand=TraduccionesProducto,UnidadesDeMedidas,CodArancelarios,Paises,CategoriasProducto"
        );
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var item = await JsonSerializer.DeserializeAsync<Classes.LSCentralProducto>(
                contentStream
            );
            return item;
        }
        else
        {
            throw new Exception("Error en el servicio de Producto de LS Central");
        }
    }

    public async Task<List<LSCentralTraducciones>> ObtenerTraduccionesProductos()
    {
        var cache = _memoryCache.Get<LSCentralTraduccionesArray>("TraduccionesProducto");
        if (cache == null)
        {
            var token = await _servicioAutorizacion.ObtenerTokenBC();
            var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
            var httpResponseMessage = await httpClient.GetAsync("TraduccionesProducto?$filter=codigo_idioma eq 'DGA'");
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                cache = await JsonSerializer.DeserializeAsync<Classes.LSCentralTraduccionesArray>(
                    contentStream
                );
                if (cache != null)
                {
                    _memoryCache.Set<LSCentralTraduccionesArray>(
                        "TraduccionesProducto",
                        cache,
                        DateTimeOffset.Now.AddMinutes(60)
                    );
                }
            }
            else
            {
                throw new Exception("Error en el servicio de Productos de LS Central");
            }
        }

        return cache
            .value.Where(i => !string.IsNullOrEmpty(i.Descripcion))
            .OrderBy(i => i.Descripcion)
            .ToList();
    }

    public async Task<List<LSCentralTraducciones>> ObtenerTraduccionesProductosPorIds(List<Guid> guids)
    {
        var TraduccionesProductos = await ObtenerTraduccionesProductos();
        if (TraduccionesProductos == null || TraduccionesProductos.Count == 0)
        {
            return new List<LSCentralTraducciones>();
        }
        return TraduccionesProductos.Where(i => guids.Contains(i.Id)).ToList();
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("Productos");
        await ObtenerProductos();
        return true;
    }
}
