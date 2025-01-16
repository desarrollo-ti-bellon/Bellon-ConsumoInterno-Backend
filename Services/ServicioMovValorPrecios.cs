
using System.Text.Json;
using Bellon.API.Liquidacion.Classes;
using Bellon.API.Liquidacion.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bellon.API.Liquidacion.Services;

public class ServicioMovValorPrecios : IServicioMovValorPrecios
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioMovValorPrecios(
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

    public async Task<List<LSCentralMovValorPrecios>> ObtenerMovValorPrecios(string no_documento)
    {
        List<LSCentralMovValorPrecios> valoresPrecios = new List<LSCentralMovValorPrecios>();
        var token = await _servicioAutorizacion.ObtenerTokenBC();
        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Liquidacion");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);

        string queryString = "";
        if (!string.IsNullOrEmpty(no_documento))
        {
            queryString = "?$filter=no_documento eq '" + no_documento + "'";
        }
        else
        {
            throw new Exception("Debe especificar un numero de recepción");
        }

        var httpResponseMessage = await httpClient.GetAsync("MovValorPrecios" + queryString);
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<LSCentralMovValorPreciosArray>(
                contentStream
            );

            if (data != null)
            {
                foreach (var item in data.value)
                {
                    valoresPrecios.Add(item);
                }
            }

            if (data.value == null || data.value.Length == 0)
            {
                throw new Exception("No hay valores de precios que mostrar");
            }

            if (data != null)
            {
                return valoresPrecios
                    .OrderByDescending(i => i.id)
                    .ThenBy(i => i.id)
                    .ToList();
            }

        }
        else
        {
            throw new Exception("Error en el servicio de MovValorPrecios de LS Central");
        }
        return [];
    }

}
