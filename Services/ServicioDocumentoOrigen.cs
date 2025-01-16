using System.Linq.Expressions;
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

public class ServicioDocumentoOrigen : IServicioDocumentoOrigen
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioDocumentoOrigen(
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

    public async Task<List<LSCentralDocumentoOrigen>> ObtenerRecepciones(string id)
    {
        List<LSCentralDocumentoOrigen> recepciones = new List<LSCentralDocumentoOrigen>();
        var token = await _servicioAutorizacion.ObtenerTokenBC();
        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Liquidacion");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
        string queryString = "";
        if (!string.IsNullOrEmpty(id))
        {
            queryString = "$filter=contains(no_factura_proveedor, '" + id + "')";
        }
        else
        {
            throw new Exception("Debe especificar un numero de factura del Proveedor");
        }

        var httpResponseMessage = await httpClient.GetAsync("QFacCompras?" + queryString);
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<Classes.LSCentralDocumentoOrigenArray>(
                contentStream
            );
            if (data != null)
            {
                foreach (var recepcion in data.value)
                {
                    var registrosConCantidadesPendientes = _context
                        .RecepcionMercancia.Where(i =>
                            i.documento_recepcion_mercancia == recepcion.NoDocumentoPrevio
                        )
                        .ToList();

                    if (recepcion.NoLineas > registrosConCantidadesPendientes.Count)
                    {
                        //EN LS HAY MAS PRODUCTOS QUE AUN NO ESTAN EN LIQUIDACION, POR LO TANTO HAY QUE MOSTRAR LA RECEPCION
                        recepciones.Add(recepcion);
                    }
                    else
                    {
                        //LA MISMA CANTIDAD DE PRODUCTOS EN LS Y EN LIQUIDACION, SE VERIFICA QUE TENGA CANTIDADES PENDIENTES POR RECIBIR
                        var itemsIncompletos = registrosConCantidadesPendientes
                            .Where(i => i.cantidad_origen > i.cantidad_recibida)
                            .Count();
                        if (itemsIncompletos > 0)
                        {
                            recepciones.Add(recepcion);
                        }
                    }
                }
                if (data.value == null || data.value.Length == 0)
                {
                    throw new Exception("No se encontró la factura del proveedor");
                }
                else if (recepciones.Count == 0)
                {
                    throw new Exception(
                        "La factura no tiene recepciones de mercancía pendientes por recibir"
                    );
                }
                else
                {
                    return recepciones
                        .OrderByDescending(i => i.NoFacturaProveedor)
                        .ThenBy(i => i.NoDocumentoPrevio)
                        .ToList();
                }
            }
        }
        else
        {
            throw new Exception("Error en el servicio de QFacCompras de LS Central");
        }
        return null;
    }

    public async Task<List<LSCentralDocumentoOrigen>> ObtenerFacturas(
        string filtroId,
        string filtroProveedor
    )
    {
        var token = await _servicioAutorizacion.ObtenerTokenBC();
        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
        string queryString = "";
        if (!string.IsNullOrEmpty(filtroId))
        {
            queryString = "$filter=contains(no_documento_origen, '" + filtroId + "')";
        }
        else if (!string.IsNullOrEmpty(filtroProveedor))
        {
            queryString = "$filter=contains(nombre_proveedor, '" + filtroProveedor + "')";
        }
        else
        {
            throw new Exception(
                "Debe especificar uno de los dos parámetros (filtroId | filtroProveedor)"
            );
        }
        var httpResponseMessage = await httpClient.GetAsync("CabeceraFactCompras?" + queryString);
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<Classes.LSCentralDocumentoOrigenArray>(
                contentStream
            );
            if (data != null)
            {
                return data.value.OrderByDescending(i => i.NoDocumentoOrigen).ToList();
            }
        }
        else
        {
            throw new Exception("Error en el servicio de CabeceraFactCompras de LS Central");
        }
        return null;
    }

    public async Task<LSCentralDocumentoOrigen> ObtenerFactura(string id, string noRecepcion)
    {
        List<LSCentralDocumentoOrigenLinea> lineas = new List<LSCentralDocumentoOrigenLinea>();
        var token = await _servicioAutorizacion.ObtenerTokenBC();
        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs");
        httpClient = _httpClientFactory.CreateClient("LSCentral-APIs");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
        var httpResponseMessage = await httpClient.GetAsync(
            "CabeceraFactCompras("
                + id
                + ")?$expand=LineaFacCompras($filter=no_documento_previo eq '"
                + noRecepcion
                + "';$expand=Productos($expand=TraduccionesProducto,UnidadesDeMedidas,CodArancelarios, Paises, CategoriasProducto)), Proveedores, Almacenes"
        );
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var item = await JsonSerializer.DeserializeAsync<Classes.LSCentralDocumentoOrigen>(
                contentStream
            );
            var detallesRecepcion = _context
                .RecepcionMercancia.Where(i => i.documento_recepcion_mercancia == noRecepcion)
                .ToList();
            foreach (var linea in item.LineaFacCompras)
            {
                var cantidadesPendientesXProducto = detallesRecepcion
                    .Where(i => i.no_producto == linea.NoProducto)
                    .FirstOrDefault();
                if (cantidadesPendientesXProducto == null)
                {
                    linea.CantidadPendiente = linea.CantidadOrigen;
                }
                else
                {
                    linea.CantidadPendiente = Convert.ToDouble(
                        cantidadesPendientesXProducto.cantidad_origen
                            - cantidadesPendientesXProducto.cantidad_recibida
                    );
                }
                if (linea.CantidadPendiente > 0)
                {
                    lineas.Add(linea);
                }
                item.LineaFacCompras = lineas.ToArray();
                foreach (var producto in linea.Productos)
                {
                    var traduccionDGA = producto
                        .TraduccionesProducto.Where(i => i.LanguageCode.ToLower().Equals("dga"))
                        .FirstOrDefault();
                    if (traduccionDGA != null)
                    {
                        producto.Descripcion = traduccionDGA.Descripcion;
                    }
                }
            }
            return item;
        }
        else
        {
            throw new Exception("Error en el servicio de CabeceraFactCompras de LS Central");
        }
    }
}
