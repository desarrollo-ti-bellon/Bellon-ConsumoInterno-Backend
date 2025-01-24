using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Bellon.API.Liquidacion.Classes;
using Bellon.API.Liquidacion.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bellon.API.Liquidacion.Services;

public class ServicioSucursal : IServicioSucursal
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioSucursal(
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

    public async Task<List<LSCentralSucursal>> ObtenerSucursales()
    {
        var cache = _memoryCache.Get<LSCentralSucursalArray>("Sucursales");
        if (cache == null)
        {
            var token = await _servicioAutorizacion.ObtenerTokenBC();
            var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
            var httpResponseMessage = await httpClient.GetAsync(
                "ValoresDimension?$filter=codigo_dimension eq 'SUCURSAL'"
            );
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                cache = await JsonSerializer.DeserializeAsync<LSCentralSucursalArray>(
                    contentStream
                );
                if (cache != null)
                {
                    _memoryCache.Set<LSCentralSucursalArray>(
                        "Sucursales",
                        cache,
                        DateTimeOffset.Now.AddMinutes(60)
                    );
                }
            }
            else
            {
                throw new Exception("Error en el servicio de Sucursales de LS Central");
            }
        }

        return cache
            .value.Where(i => !string.IsNullOrEmpty(i.Nombre))
            .OrderBy(i => i.Nombre)
            .ToList();
    }

    public async Task<LSCentralSucursal> ObtenerSucursal(string id)
    {
        var token = await _servicioAutorizacion.ObtenerTokenBC();
        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
        var httpResponseMessage = await httpClient.GetAsync(
            "ValoresDimension?$filter=codigo_dimension eq 'SUCURSAL'"
        );
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var item = await JsonSerializer.DeserializeAsync<LSCentralSucursal>(
                contentStream
            );
            return item;
        }
        else
        {
            throw new Exception("Error en el servicio de Sucursal de LS Central");
        }
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("Sucursales");
        await ObtenerSucursales();
        return true;
    }
}
