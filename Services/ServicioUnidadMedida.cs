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

public class ServicioUnidadMedida : IServicioUnidadMedida
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioUnidadMedida(
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

    public async Task<List<LSCentralUnidadMedida>> ObtenerUnidadesMedida(string? filtro)
    {
        var cache = _memoryCache.Get<LSCentralUnidadMedidaArray>("UnidadesMedida");
        if (cache == null)
        {
            var token = await _servicioAutorizacion.ObtenerTokenBC();
            var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
            var httpResponseMessage = await httpClient.GetAsync("UnidadesDeMedidas");
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                cache = await JsonSerializer.DeserializeAsync<Classes.LSCentralUnidadMedidaArray>(
                    contentStream
                );
                if (cache != null)
                {
                    _memoryCache.Set<LSCentralUnidadMedidaArray>(
                        "UnidadesMedida",
                        cache,
                        DateTimeOffset.Now.AddMinutes(60)
                    );
                }
            }
            else
            {
                throw new Exception("Error en el servicio de UnidadesDeMedidas de LS Central");
            }
        }
        if (string.IsNullOrEmpty(filtro))
        {
            return cache.value.OrderBy(i => i.Codigo).ToList();
        }
        else
        {
            return cache
                .value.Where(i => i.Codigo.ToLower().Contains(filtro.ToLower()))
                .OrderBy(i => i.Codigo)
                .ToList();
        }
    }

    public async Task<LSCentralUnidadMedida> ObtenerUnidadMedida(string id)
    {
        if (String.IsNullOrEmpty(id))
        {
            throw new InvalidDataException("El id de la unidad de medida no puede ser nulo");
        }

        var token = await _servicioAutorizacion.ObtenerTokenBC();
        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
        httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
        var httpResponseMessage = await httpClient.GetAsync("UnidadesDeMedidas(" + id + ")");
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var item = await JsonSerializer.DeserializeAsync<Classes.LSCentralUnidadMedida>(
                contentStream
            );
            return item;
        }
        else
        {
            throw new Exception("Error en el servicio de UnidadesDeMedidas de LS Central");
        }
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("UnidadesMedida");
        await ObtenerUnidadesMedida(string.Empty);
        return true;
    }

}
