using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Bellon.API.Liquidacion.Classes;
using Bellon.API.Liquidacion.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bellon.API.Liquidacion.Services;

public class ServicioOrdenTransferencia : IServicioOrdenTransferencia
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;
    private readonly IServicioAlmacen _servicioAlmacen;

    public ServicioOrdenTransferencia(
        IHttpContextAccessor httpContextAccessor,
        DataBase.AppDataBase context,
        IOptions<AppSettings> settings,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IServicioAutorizacion servicioAutorizacion,
        IServicioAlmacen servicioAlmacen
    )
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _servicioAutorizacion = servicioAutorizacion;
        _servicioAlmacen = servicioAlmacen;
    }

    public async Task<List<OrdenTransferencia>> ObtenerOrdenes()
    {
        var cache = _memoryCache.Get<List<OrdenTransferencia>>("OrdenesTransferencia");
        if (cache == null)
        {
            cache = _context
                .OrdenesTransferencia.Select(i => new OrdenTransferencia
                {
                    CreadoPor = i.creado_por,
                    FechaCreado = i.fecha_creado,
                    FechaModificado = i.fecha_modificado,
                    HistCabeceraLiquidacionId = i.hist_cabecera_liquidacion_id,
                    HistCabeceraTransitoId = i.hist_cabecera_transito_id,
                    IdOrdenEnvio = i.id_orden_envio,
                    IdOrdenTransferencia = i.id_orden_transferencia,
                    ModificadoPor = i.modificado_por,
                    NoOrdenEnvio = i.no_orden_envio,
                    NoOrdenTransferencia = i.no_orden_transferencia,
                })
                .ToList();
            _memoryCache.Set<List<OrdenTransferencia>>(
                "OrdenesTransferencia",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return cache.OrderBy(i => i.IdOrdenTransferencia).ToList();
    }

    public async Task<List<OrdenTransferencia>> ObtenerOrdenes(int idLiquidacion)
    {
        var allItems = await ObtenerOrdenes();
        return allItems.Where(i => i.HistCabeceraLiquidacionId == idLiquidacion).ToList();
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("OrdenesTransferencia");
        await ObtenerOrdenes();
        return true;
    }

    public async Task<Resultado> CrearTransferencias(int idLiquidacion)
    {
        var transferencias = _context
            .OrdenesTransferencia.Where(i => i.hist_cabecera_liquidacion_id == idLiquidacion)
            .Count();

        if (transferencias == 0)
        {
            var lineasLiquidacion = _context
                .LineaLiquidaciones.Where(i => i.cabecera_liquidacion_id == idLiquidacion)
                .ToList();
            if (lineasLiquidacion != null && lineasLiquidacion.Count > 0)
            {
                var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                var todosAlmacenes = await _servicioAlmacen.ObtenerAlmacenes();
                var transitos = lineasLiquidacion
                    .Select(i => i.hist_cabecera_transito_id)
                    .Distinct();
                foreach (var transitoId in transitos)
                {
                    var linea = lineasLiquidacion
                        .Where(i => i.hist_cabecera_transito_id == transitoId)
                        .FirstOrDefault();
                    var almacenDestino = todosAlmacenes
                        .Where(i => i.IdAlmacen.ToString() == linea.almacen_id)
                        .FirstOrDefault();
                    var almacenOrigen = todosAlmacenes
                        .Where(i => i.Codigo == "CDI")
                        .FirstOrDefault();
                    if (almacenDestino != null && almacenOrigen != null)
                    {
                        //SE CREA LA CABECERA DEL OBJETO
                        var transferencia = new LSCentralTransferencia
                        {
                            TransferFromCode = almacenOrigen.Codigo,
                            TransferToCode = almacenDestino.Codigo,
                            PostingDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                            ShipmentDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                            ReceiptDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                            NoSeries = "TRF-REQ",
                            TransferFromContact = almacenOrigen.Contact,
                            AssignedUserID = identity!.Name.Split("@")[0].ToUpper(),
                            LscBuyerId = identity!.Name.Split("@")[0].ToUpper(),
                            InTransitCode = "TRANSITO",
                        };
                        //OBTIENE TODAS LAS LINEAS DE LA LIQUIDACIÓN QUE CORRESPONDEN AL TRANSITO
                        var lineasxTransito = lineasLiquidacion
                            .Where(i => i.hist_cabecera_transito_id == transitoId)
                            .ToList()
                            .Clone();

                        //SE AGRUPAN LAS LINEAS DEL ALMACEN POR ID Y SE CALCULA LA SUMATORIA DE CANTIDADES
                        //SE ASIGNAN AL DETALLE DE LA CABECERA
                        transferencia.TransferLines = lineasxTransito
                            .GroupBy(l => l.no_producto)
                            .Select(cl => new LSCentralTransferenciaLinea
                            {
                                ItemNo = cl.Key,
                                QtyToShip = cl.Sum(c => c.cantidad),
                                Quantity = cl.Sum(c => c.cantidad),
                                UnitOfMeasureCode = cl.First().codigo_unidad_medida,
                                ReceiptDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                                ShipmentDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                            })
                            .ToArray();

                        //SE ASIGNA EL CONSECUTIVO
                        for (int i = 1; i <= transferencia.TransferLines.Length; i++)
                        {
                            transferencia.TransferLines[i - 1].LineNo = 10000 * i;
                        }

                        //SE LLAMAN LAS APIs
                        var token = await _servicioAutorizacion.ObtenerTokenBC();
                        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Liquidacion");
                        httpClient.DefaultRequestHeaders.Add(
                            "Authorization",
                            "Bearer " + token!.AccessToken
                        );
                        var stringContent = JsonSerializer.Serialize<LSCentralTransferencia>(
                            transferencia
                        );

                        var buffer = System.Text.Encoding.UTF8.GetBytes(stringContent);
                        var byteContent = new ByteArrayContent(buffer);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue(
                            "application/json"
                        );

                        var httpResponseMessage = await httpClient.PostAsync(
                            "TransferOrders?$expand=transferlines",
                            byteContent
                        );
                        if (httpResponseMessage.IsSuccessStatusCode)
                        {
                            using var contentStream =
                                await httpResponseMessage.Content.ReadAsStreamAsync();
                            var response =
                                await JsonSerializer.DeserializeAsync<Classes.LSCentralTransferencia>(
                                    contentStream
                                );

                            httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Liquidacion");
                            httpClient.DefaultRequestHeaders.Add(
                                "Authorization",
                                "Bearer " + token!.AccessToken
                            );
                            httpResponseMessage = await httpClient.PostAsync(
                                "TransferOrders("
                                    + response.Id.ToString().Replace("{", "").Replace("}", "")
                                    + ")/Microsoft.NAV.postShipment",
                                null
                            );
                            if (httpResponseMessage.IsSuccessStatusCode)
                            {
                                httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Liquidacion");
                                httpClient.DefaultRequestHeaders.Add(
                                    "Authorization",
                                    "Bearer " + token!.AccessToken
                                );
                                httpResponseMessage = await httpClient.GetAsync(
                                    "TransferShipments?$filter=transferOrderNo eq '"
                                        + response.No
                                        + "'"
                                );
                                if (httpResponseMessage.IsSuccessStatusCode)
                                {
                                    using var contentStreamShipment =
                                        await httpResponseMessage.Content.ReadAsStreamAsync();
                                    var shipment =
                                        await JsonSerializer.DeserializeAsync<Classes.LSCentralTransferenciaArray>(
                                            contentStreamShipment
                                        );
                                    var nuevaOrden = new DataBase.Models.OrdenesTransferencia
                                    {
                                        id_orden_transferencia = response
                                            .Id.ToString()
                                            .Replace("{", "")
                                            .Replace("}", ""),
                                        no_orden_transferencia = response.No,
                                        hist_cabecera_transito_id = transitoId,
                                        hist_cabecera_liquidacion_id = idLiquidacion,
                                        id_orden_envio = shipment.value[0].Id.ToString(),
                                        no_orden_envio = shipment.value[0].No,
                                        fecha_creado = DateTime.UtcNow,
                                        creado_por = identity!.Name,
                                    };
                                    _context.OrdenesTransferencia.Add(nuevaOrden);
                                    _context.SaveChanges();
                                }
                                else
                                {
                                    throw new Exception(
                                        "Error en el servicio de TransferShipments de LS Central"
                                    );
                                }
                            }
                            else
                            {
                                throw new Exception(
                                    "Error en el servicio de TransferOrders de LS Central"
                                );
                            }
                        }
                        else
                        {
                            throw new Exception(
                                "Error en el servicio de TransferOrders de LS Central"
                            );
                        }
                    }
                    else
                    {
                        return new Resultado
                        {
                            Exito = false,
                            Mensaje = string.Format(
                                "No se encontró el almacén {0}",
                                linea.almacen_id
                            ),
                        };
                    }
                }
                await RefrescarCache();
                return new Resultado { Exito = true };
            }
            else
            {
                return new Resultado
                {
                    Exito = false,
                    Mensaje = string.Format(
                        "No se encontró la Liquidación {0} para transferir",
                        idLiquidacion.ToString()
                    ),
                };
            }
        }
        else
        {
            return new Resultado { Exito = true };
        }
    }
}
