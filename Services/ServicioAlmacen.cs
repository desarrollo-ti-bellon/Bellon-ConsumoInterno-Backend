using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using Bellon.API.ConsumoInterno.Classes;
using Bellon.API.ConsumoInterno.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bellon.API.ConsumoInterno.Services;

public class ServicioAlmacen : IServicioAlmacen
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioAlmacen(
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

    public async Task<List<LSCentralAlmacen>> ObtenerAlmacenes()
    {
        var cache = _memoryCache.Get<LSCentralAlmacenArray>("Almacenes");
        if (cache == null)
        {
            var token = await _servicioAutorizacion.ObtenerTokenBC();
            var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
            var httpResponseMessage = await httpClient.GetAsync("Almacenes");
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                cache = await JsonSerializer.DeserializeAsync<Classes.LSCentralAlmacenArray>(
                    contentStream
                );
                if (cache != null)
                {
                    _memoryCache.Set<LSCentralAlmacenArray>(
                        "Almacenes",
                        cache,
                        DateTimeOffset.Now.AddMinutes(60)
                    );
                }
            }
            else
            {
                throw new Exception("Error en el servicio de Almacenes de LS Central");
            }
        }
        return cache.value.OrderBy(i => i.Codigo).ToList();
    }

    public async Task<LSCentralAlmacen> ObtenerAlmacen(string id)
    {
        if (String.IsNullOrEmpty(id))
        {
            throw new InvalidDataException("El id del almacén no puede ser nulo o vacío");
        }

        var token = await _servicioAutorizacion.ObtenerTokenBC();
        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
        httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
        var httpResponseMessage = await httpClient.GetAsync("Almacenes(" + id + ")");
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var item = await JsonSerializer.DeserializeAsync<Classes.LSCentralAlmacen>(
                contentStream
            );
            return item;
        }
        else
        {
            throw new Exception("Error en el servicio de Almacenes de LS Central");
        }
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("Almacenes");
        await ObtenerAlmacenes();
        return true;
    }

}
