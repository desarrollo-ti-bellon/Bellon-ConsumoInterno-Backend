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
using System.Net;
using System.Text;

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
        return cache.value.OrderBy(i => i.NoDocumento).ToList();
    }

    public async Task<LSCentralAjusteInventario> ObtenerAjusteDeInventario(string no_documento)
    {
        var allItems = await ObtenerAjustesDeInventarios();
        return allItems.Where(i => i.NoDocumento == no_documento).FirstOrDefault();
    }

    public async Task<Resultado> CrearAjusteInventario(int? idSolicitud)
    {

        var client = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
        var url = client?.BaseAddress?.ToString();
        var idCompania = url?.Split(new[] { "companies(", ")" }, StringSplitOptions.None)[1];

        if (idSolicitud.HasValue)
        {

            var solicitud = await _context.CabeceraConsumosInternos.Where(i => i.id_cabecera_consumo_interno == idSolicitud).FirstOrDefaultAsync();
            var listadoProductos = await _context.LineasConsumosInternos.Where(i => i.cabecera_consumo_interno_id == idSolicitud).ToListAsync();

            if (listadoProductos.Count > 0)
            {
                var indice = 1;
                var listadoProductosLS = new List<LSCentralAjusteInventario>();
                foreach (var item in listadoProductos)
                {
                    var ajusteInventario = new LSCentralAjusteInventario
                    {
                        NoDocumento = solicitud.no_documento,
                        NombreDiario = "ITEM",
                        NombreSeccionDiario = "AJ.COIN",
                        NoLinea = indice * 1000,
                        // TipoMovimiento = "Negative Adjmt.",
                        NoProducto = item.no_producto,
                        Cantidad = item.cantidad,
                        CodigoAlmacen = item.almacen_codigo,
                    };

                    listadoProductosLS.Add(ajusteInventario);

                    indice++; // GENERANDO EL NUMERO DE LINEA
                }

                var httpClient = new HttpClient();
                var token = await _servicioAutorizacion.ObtenerTokenBC();
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    _settings.LSCentralQueryComunes + "ItemJournalIntegrationHandler_InsertLine"
                );

                request.Headers.Add("company", idCompania);
                request.Headers.Add("Authorization", "Bearer " + token.AccessToken);

                var json = new
                {
                    lineas = listadoProductosLS
                };

                var stringContentArray = JsonSerializer.Serialize(json);
                var requestJson = new LSCentralRequest { RequestJson = stringContentArray };
                var stringContent = JsonSerializer.Serialize<LSCentralRequest>(requestJson);
                var content = new StringContent(stringContent, Encoding.UTF8, "application/json");
                request.Content = content;
                var response = await httpClient.SendAsync(request);

                var mensajeError = new StringBuilder();
                if (response.StatusCode == HttpStatusCode.OK)
                {

                    var mensaje = await response.Content.ReadAsStringAsync();
                    var respuesta = JsonSerializer.Deserialize<LSCentralAjusteInventarioRespuesta>(mensaje);
                    var respuestaLS = JsonSerializer.Deserialize<LSCentralAjusteInventarioValor>(respuesta.Value);
                    var respuestaLSArray = respuestaLS.Result;

                    if (respuestaLS.Exito == false)
                    {
                        if (respuestaLSArray.Count > 0)
                        {
                            foreach (var item in respuestaLSArray)
                            {
                                if (item.Exito == false)
                                {
                                    mensajeError.AppendLine($"<li> Producto: {item.NoProducto} - {item.Mensaje} </li>");
                                }
                            }
                        }
                    }

                }
                else
                {
                    return new Resultado
                    {
                        Exito = false,
                        Mensaje = "No se pudieron realizar los ajustes de inventario en el LS Central"
                    };
                }

                if (mensajeError.Length == 0)
                {
                    return new Resultado
                    {
                        Exito = true,
                        Mensaje = "Se realizaron todos los ajustes de inventarios correctamente."
                    };
                }
                else
                {
                    return new Resultado
                    {
                        Exito = false,
                        Mensaje = mensajeError.ToString()
                    };
                }
            }
            else
            {
                return new Resultado
                {
                    Exito = false,
                    Mensaje = "Este documento no tiene productos agregados en la solicitud de consumos internos"
                };
            }
        }
        else
        {
            return new Resultado
            {
                Exito = false,
                Mensaje = "debe de especificar el codigo de cabecera de la solicitud."
            };
        }

    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("AjusteInventarios");
        await ObtenerAjustesDeInventarios();
        return true;
    }

}
