using System.Security.Claims;
using System.Text.Json;
using Bellon.API.Liquidacion.Classes;
using Bellon.API.Liquidacion.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bellon.API.Liquidacion.Services;

public class ServicioCargoFactura : IServicioCargoFactura
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioCargoFactura(
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

    public async Task<List<CargoFacturaLiquidacion>> ObtenerCargoFacturas()
    {
        var cache = _memoryCache.Get<List<CargoFacturaLiquidacion>>("CargoFacturaLiquidacion");
        if (cache == null)
        {
            cache = _context.CargoFacturaLiquidacion.Select(i => new CargoFacturaLiquidacion
            {
                IdCargoFacturaLiquidacion = i.id_cargo_factura_liquidacion,
                NoCargoProducto = i.no_cargo_producto,
                NoFactura = i.no_factura,
                NoProveedor = i.no_proveedor,
                NombreProveedor = i.nombre_proveedor,
                NoDocumentoLiquidacion = i.no_documento_liquidacion,
                CabeceraLiquidacionId = i.cabecera_liquidacion_id,
                IdFactura = i.id_factura,
                Fecha = i.fecha,
                Importe = i.importe,
                ImporteSinIVA = i.importe_sin_iva,
                ReferenciaCargoProducto = i.referencia_cargo_producto
            })
                .ToList();
            _memoryCache.Set<List<CargoFacturaLiquidacion>>(
                "CargoFacturaLiquidacion",
                cache,
                DateTimeOffset.Now.AddMinutes(5)
            );
        }
        return cache;
    }

    public async Task<List<CargoFacturaLiquidacion>> ObtenerCargoFactura(int id)
    {
        var allItems = await ObtenerCargoFacturas();
        var item = allItems.Where(i => i.CabeceraLiquidacionId == id).FirstOrDefault().Clone();
        if (item == null)
        {
            allItems = _context.CargoFacturaLiquidacion
                 .Where(i => i.cabecera_liquidacion_id == id)
                 .Select(i => new CargoFacturaLiquidacion
                 {
                     IdCargoFacturaLiquidacion = i.id_cargo_factura_liquidacion,
                     NoCargoProducto = i.no_cargo_producto,
                     NoFactura = i.no_factura,
                     NoProveedor = i.no_proveedor,
                     NombreProveedor = i.nombre_proveedor,
                     NoDocumentoLiquidacion = i.no_documento_liquidacion,
                     CabeceraLiquidacionId = i.cabecera_liquidacion_id,
                     IdFactura = i.id_factura,
                     Fecha = i.fecha,
                     Importe = i.importe,
                     ImporteSinIVA = i.importe_sin_iva,
                     ReferenciaCargoProducto = i.referencia_cargo_producto
                 })
                 .ToList();
        }
        return allItems;
    }

    public async Task<List<CargoFacturaLiquidacion>> GuardarCargoFactura(List<CargoFacturaLiquidacion> items)
    {
        if (items.Count > 0)
        {
            var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            foreach (var item in items)
            {
                if (!item.IdCargoFacturaLiquidacion.HasValue)
                {
                    //SE INSERTA SOLOS LOS NUEVO ITEMS
                    var newItemData = new DataBase.Models.CargosFacturaLiquidacion
                    {
                        no_cargo_producto = item.NoCargoProducto,
                        no_factura = item.NoFactura,
                        no_proveedor = item.NoProveedor ?? throw new Exception("El campo 'NoProveedor' es obligatorio."),
                        nombre_proveedor = item.NombreProveedor ?? throw new Exception("El campo 'NombreProveedor' es obligatorio."),
                        no_documento_liquidacion = item.NoDocumentoLiquidacion,
                        cabecera_liquidacion_id = item.CabeceraLiquidacionId,
                        id_factura = item.IdFactura,
                        fecha = DateTime.UtcNow,
                        importe = item.Importe,
                        importe_sin_iva = item.ImporteSinIVA,
                        referencia_cargo_producto = item.ReferenciaCargoProducto,
                        fecha_creado = DateTime.UtcNow,
                        creado_por = identity!.Name!
                    };
                    var newItem = _context.CargoFacturaLiquidacion.Add(newItemData);
                    try
                    {
                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error al crear el registro: <" + ex.Message + ">");
                    }
                }
            }
            //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
            await RefrescarCache();
            //SE RETORNA EL OBJETO CREADO
            return await ObtenerCargoFactura(items[0].CabeceraLiquidacionId);
        }
        else
        {
            throw new Exception("No hay items para insertar");
        }
    }

    public async Task<List<LSCentralCargoFactura>> ObtenerCargoFacturasERP()
    {
        var cache = _memoryCache.Get<LSCentralCargoFacturaArray>("CargoFacturaLiquidacionERP");
        if (cache == null)
        {
            var token = await _servicioAutorizacion.ObtenerTokenBC();
            var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Liquidacion");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
            var httpResponseMessage = await httpClient.GetAsync("QCargoFacturas");
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                cache =
                    await JsonSerializer.DeserializeAsync<LSCentralCargoFacturaArray>(
                        contentStream
                    );
                if (cache != null)
                {
                    _memoryCache.Set<LSCentralCargoFacturaArray>(
                        "CargoFacturaLiquidacionERP",
                        cache,
                        DateTimeOffset.Now.AddMinutes(60)
                    );
                }
            }
            else
            {
                throw new Exception("Error en el servicio de CargoFacturas de LS Central");
            }
        }
        return cache
            .value
            .OrderBy(i => i.NoFactura)
            .ToList();
    }

    public async Task<LSCentralCargoFacturaArray> ObtenerCargoFacturaERP(string id)
    {
        var token = await _servicioAutorizacion.ObtenerTokenBC();
        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Liquidacion");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);

        string queryString = string.IsNullOrEmpty(id) ? throw new Exception("Debe especificar un numero de factura") : $"?$filter=no_factura eq '{Uri.EscapeDataString(id)}'";

        var httpResponseMessage = await httpClient.GetAsync("QCargoFacturas" + queryString);
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<LSCentralCargoFacturaArray>(contentStream);

            if (data == null)
            {
                throw new Exception("No se encontró la factura especificada");
            }

            var cargosLiquidacion = data.value.Where(item => !_context.CargoFacturaLiquidacion.Any(i => i.id_factura == item.IdLinea));
            data.value = cargosLiquidacion.ToArray();
            return data;
        }
        else
        {
            throw new Exception("Error en el servicio de CargoFacturas de LS Central");
        }

    }

    public async Task<List<CargoFacturaLiquidacion>> EliminarLineaCargoFactura(int id)
    {
        //ELIMINA TODAS LAS LINEAS QUE TENGAN EL TRANSITO (item)
        var item = _context.CargoFacturaLiquidacion.Single(cargo => cargo.id_cargo_factura_liquidacion == id);
        if (item != null)
        {
            _context.CargoFacturaLiquidacion.Remove(item);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el registro: <" + ex.Message + ">");
            }

            //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
            await RefrescarCache();

            return await ObtenerCargoFactura(item.cabecera_liquidacion_id);
        }
        return null;
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("CargoFacturaLiquidacion");
        _memoryCache.Remove("CargoFacturaLiquidacionERP");
        await ObtenerCargoFacturas();
        await ObtenerCargoFacturasERP();
        return true;
    }
}
