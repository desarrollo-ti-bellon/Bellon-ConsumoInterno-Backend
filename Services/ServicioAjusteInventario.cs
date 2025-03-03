using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using Bellon.API.ConsumoInterno.Classes;
using Bellon.API.ConsumoInterno.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Bellon.API.ConsumoInterno.Services;

public class ServicioAjusteInventario : IServicioAjusteInventario
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioAjusteInventario(
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

    public async Task<List<LSCentralAjusteInventario>> ObtenerAjustesDeInventarios()
    {
        var cache = _memoryCache.Get<LSCentralAjusteInventarioArray>("AjusteInventarios");
        if (cache == null)
        {
            var token = await _servicioAutorizacion.ObtenerTokenBC();
            var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
            var httpResponseMessage = await httpClient.GetAsync("LineasDiarioProducto");
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                cache = await JsonSerializer.DeserializeAsync<Classes.LSCentralAjusteInventarioArray>(
                    contentStream
                );
                if (cache != null)
                {
                    _memoryCache.Set<LSCentralAjusteInventarioArray>(
                        "AjusteInventarios",
                        cache,
                        DateTimeOffset.Now.AddMinutes(60)
                    );
                }
            }
            else
            {
                throw new Exception("Error en el servicio de AjusteInventarios de LS Central");
            }
        }
        return cache.value.OrderBy(i => i.NoOrden).ToList();
    }

    public async Task<LSCentralAjusteInventario> ObtenerAjusteDeInventario(string no_documento)
    {
        var allItems = await ObtenerAjustesDeInventarios();
        return allItems.Where(i => i.NoOrden == no_documento).FirstOrDefault();
    }

    public async Task<bool> CrearAjusteInventario(int? idSolicitud)
    {

        var resultado = false;
        if (idSolicitud.HasValue)
        {

            var solicitud = await _context.CabeceraConsumosInternos.Where(i => i.id_cabecera_consumo_interno == idSolicitud).FirstOrDefaultAsync();
            var listadoProductos = await _context.LineasConsumosInternos.Where(i => i.cabecera_consumo_interno_id == idSolicitud).ToListAsync();

            if (listadoProductos.Count > 0)
            {
                var indice = 1;
                foreach (var item in listadoProductos)
                {
                    // ----------------------------------------------------------------
                    var ajusteInventario = new LSCentralAjusteInventario
                    {
                        FechaRegistro = DateOnly.FromDateTime(DateTime.Now),
                        FechaDocumento = DateOnly.Parse(solicitud.fecha_creado.ToString("yyyy-MM-dd")),
                        NoOrden = solicitud.no_documento,
                        NombreDiario = "ITEM",
                        NombreSeccionDiario = "AJ.COIN",
                        CodigoAuditoria = "USO INT.",
                        NoLinea = indice * 10000,
                        CodigoAlmacen = item.almacen_codigo,
                        TipoMovimiento = "Negative Adjmt.",
                        NoProducto = item.no_producto,
                        Cantidad = item.cantidad,
                    };
                    // ----------------------------------------------------------------
                    indice++;
                    // ----------------------------------------------------------------

                    //SE LLAMAN LAS APIs
                    var token = await _servicioAutorizacion.ObtenerTokenBC();
                    var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
                    var stringContent = JsonSerializer.Serialize<LSCentralAjusteInventario>(
                        ajusteInventario
                    );

                    var buffer = System.Text.Encoding.UTF8.GetBytes(stringContent);
                    var byteContent = new ByteArrayContent(buffer);
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue(
                        "application/json"
                    );

                    var httpResponseMessage = await httpClient.PostAsync(
                        "LineasDiarioProducto",
                        byteContent
                    );

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                        var response = await JsonSerializer.DeserializeAsync<Classes.LSCentralAjusteInventario>(contentStream);
                    }
                    else
                    {
                        throw new Exception("Error en el servicio de ajuste de inventario consumos internos de LS Central");
                    }
                }
                resultado = true;
            }
            else
            {
                throw new Exception("Este documento no tiene productos agregados en la solicitud de consumos internos");
            }
        }
        else
        {
            throw new Exception("debe de especificar el codigo de cabecera de la solicitud.");
        }

        return resultado;
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("AjusteInventarios");
        await ObtenerAjustesDeInventarios();
        return true;
    }

}
