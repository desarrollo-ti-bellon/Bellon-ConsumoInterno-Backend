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

public class ServicioDepartamento : IServicioDepartamento
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioDepartamento(
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

    public async Task<List<LSCentralDepartamento>> ObtenerDepartamentos()
    {
        var cache = _memoryCache.Get<LSCentralDepartamentoArray>("Departamentos");
        if (cache == null)
        {
            var token = await _servicioAutorizacion.ObtenerTokenBC();
            var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
            var httpResponseMessage = await httpClient.GetAsync(
                "ValoresDimension?$filter=codigo_dimension eq 'DPTO-OP'"
            );
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                cache = await JsonSerializer.DeserializeAsync<LSCentralDepartamentoArray>(
                    contentStream
                );
                if (cache != null)
                {
                    _memoryCache.Set<LSCentralDepartamentoArray>(
                        "Departamentos",
                        cache,
                        DateTimeOffset.Now.AddMinutes(60)
                    );
                }
            }
            else
            {
                throw new Exception("Error en el servicio de Departamentos de LS Central");
            }
        }

        return cache
            .value.Where(i => !string.IsNullOrEmpty(i.Nombre))
            .OrderBy(i => i.Nombre)
            .ToList();
    }

    public async Task<LSCentralDepartamento> ObtenerDepartamento(string id)
    {
        var token = await _servicioAutorizacion.ObtenerTokenBC();
        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
        var httpResponseMessage = await httpClient.GetAsync(
            "ValoresDimension?$filter=codigo_dimension eq 'DPTO-OP'"
        );
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var item = await JsonSerializer.DeserializeAsync<LSCentralDepartamento>(
                contentStream
            );
            return item;
        }
        else
        {
            throw new Exception("Error en el servicio de Departamento de LS Central");
        }
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("Departamentos");
        await ObtenerDepartamentos();
        return true;
    }
}
