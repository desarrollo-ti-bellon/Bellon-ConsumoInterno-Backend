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

public class ServicioHistTransito : IServicioHistTransito
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;

    private readonly IOptions<AppSettings> _settingsIO;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioHistTransito(
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
        _settingsIO = settings;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _servicioAutorizacion = servicioAutorizacion;
    }

    public async Task<List<HistCabeceraTransito>> ObtenerTransitos()
    {
        var cache = _memoryCache.Get<List<HistCabeceraTransito>>("HistTransitos");
        if (cache == null)
        {
            cache = _context
                .HistCabeceraTransitos.Select(i => new HistCabeceraTransito
                {
                    IdHistCabeceraTransito = i.id_hist_cabecera_transito,
                    AlmacenCodigo = i.almacen_codigo,
                    AlmacenId = i.almacen_id,
                    DetalleMercancia = i.detalle_mercancia,
                    FechaDesembarque = i.fecha_desembarque,
                    FechaDocumento = i.fecha_documento,
                    FechaEmbarque = i.fecha_embarque,
                    FechaEstimada = i.fecha_estimada,
                    Naviera = i.naviera,
                    NoBuque = i.no_buque,
                    NoConocimientoEmbarque = i.no_conocimiento_embarque,
                    NoContenedor = i.no_contenedor,
                    NoDocumento = i.no_documento,
                    NombreProveedor = i.nombre_proveedor,
                    NoSerieId = i.no_serie_id,
                    NoSello = i.no_sello,
                    PuertoDesembarque = i.puerto_desembarque,
                    PuertoEmbarque = i.puerto_embarque,
                    TipoContenedor_id = i.tipo_contenedor_id,
                    Total = i.total,
                    FechaCreado = i.fecha_creado,
                    FechaModificado = i.fecha_modificado,
                    FechaRegistro = i.fecha_registro,
                    ModificadoPor = i.modificado_por,
                    CreadoPor = i.creado_por,
                    CantidadLineas = i.HistLineaTransitos.Count,
                })
                .ToList();
            _memoryCache.Set<List<HistCabeceraTransito>>(
                "HistTransitos",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return cache.OrderBy(i => i.IdHistCabeceraTransito.Value).ToList();
        ;
    }

    public async Task<List<HistCabeceraTransito>> ObtenerTransitos(int idLiquidacion)
    {
        var todosTransitos = await ObtenerTransitos();
        if (todosTransitos != null)
        {
            var _servicioLiquidacion = new ServicioLiquidacion(
                _httpContextAccessor,
                _context,
                _settingsIO,
                _httpClientFactory,
                _memoryCache,
                _servicioAutorizacion,
                null,
                this,
                null,
                null,
                null
            );
            var _servicioLiquidacionHistorico = new ServicioHistLiquidacion(
                _httpContextAccessor,
                _context,
                _settingsIO,
                _httpClientFactory,
                _memoryCache,
                _servicioAutorizacion
            );
            var liquidacion = await _servicioLiquidacion.ObtenerLiquidacion(idLiquidacion);
            if (liquidacion != null)
            {
                var transitosID = liquidacion
                    .Lineas!.Select(i => i.HistCabeceraTransitoId)
                    .Distinct()
                    .ToList();
                return todosTransitos
                    .Where(i => transitosID.Contains(i.IdHistCabeceraTransito!.Value))
                    .ToList();
            }
            else
            {
                var liquidacionHistorica = await _servicioLiquidacionHistorico.ObtenerLiquidacion(
                    idLiquidacion
                );
                var transitosIDHistorica = liquidacionHistorica
                    .Lineas!.Select(i => i.HistCabeceraTransitoId)
                    .Distinct()
                    .ToList();
                return todosTransitos
                    .Where(i => transitosIDHistorica.Contains(i.IdHistCabeceraTransito!.Value))
                    .ToList();
            }
        }
        return null;
    }

    public async Task<HistCabeceraTransito> ObtenerTransito(int id)
    {
        var allItems = await ObtenerTransitos();
        var item = allItems.Where(i => i.IdHistCabeceraTransito == id).FirstOrDefault().Clone();
        if (item != null)
        {
            item.Lineas = _context
                .HistLineaTransitos.Where(i => i.hist_cabecera_transito_id == id)
                .Select(i => new HistLineaTransito
                {
                    IdHistLineaTransito = i.id_hist_linea_transito,
                    HistCabeceraTransitoId = i.hist_cabecera_transito_id,
                    HistCabeceraLlegadaId = i.hist_cabecera_llegada_id,
                    FechaModificado = i.fecha_modificado,
                    ModificadoPor = i.modificado_por,
                    NoLlegada = i.no_llegada,
                    CreadoPor = i.creado_por,
                    FechaCreado = i.fecha_creado,
                    HistCabeceraLlegada = new HistCabeceraLlegada
                    {
                        IdHistCabeceraLlegada = i.hist_cabecera_llegada.id_hist_cabecera_llegada,
                        NoDocumento = i.hist_cabecera_llegada.no_documento,
                        FechaDocumento = i.hist_cabecera_llegada.fecha_documento,
                        NoDocumentoOrigen = i.hist_cabecera_llegada.no_documento_origen,
                        NoFacturaProveedor = i.hist_cabecera_llegada.no_factura_proveedor,
                        NoProveedor = i.hist_cabecera_llegada.no_proveedor,
                        NombreProveedor = i.hist_cabecera_llegada.nombre_proveedor,
                        NoAlmacenUS = i.hist_cabecera_llegada.no_almacen_us,
                        Total = i.hist_cabecera_llegada.total,
                        NombreAgente = i.hist_cabecera_llegada.agente.nombre,
                        CreadoPor = i.hist_cabecera_llegada.creado_por,
                        AgenteId = i.hist_cabecera_llegada.agente_id,
                        CantidadPeso = i.hist_cabecera_llegada.cantidad_peso,
                        CantidadPieza = i.hist_cabecera_llegada.cantidad_pieza,
                        CantidadVolumen = i.hist_cabecera_llegada.cantidad_volumen,
                        CodigoPeso = i.hist_cabecera_llegada.codigo_peso,
                        CodigoVolumen = i.hist_cabecera_llegada.codigo_volumen,
                        FechaCreado = i.hist_cabecera_llegada.fecha_creado,
                        FechaModificado = i.hist_cabecera_llegada.fecha_modificado,
                        FechaRegistro = i.hist_cabecera_llegada.fecha_registro,
                        IdDocumentoOrigen = i.hist_cabecera_llegada.id_documento_origen,
                        IdPeso = i.hist_cabecera_llegada.id_peso,
                        IdProveedor = i.hist_cabecera_llegada.id_proveedor,
                        IdVolumen = i.hist_cabecera_llegada.id_volumen,
                        ModificadoPor = i.hist_cabecera_llegada.modificado_por,
                        NoDocumentoPrevio = i.hist_cabecera_llegada.no_documento_previo,
                        NoSerieId = i.hist_cabecera_llegada.no_serie_id,
                        Transportista = i.hist_cabecera_llegada.transportista,
                    },
                })
                .OrderBy(i => i.IdHistLineaTransito)
                .ToList();
        }
        return item;
    }

    public async Task<Resultado> Recuperar(int id)
    {
        var histItem = await ObtenerTransito(id);
        bool existeTransaccion = _context.Database.CurrentTransaction == null ? false : true;
        if (histItem != null)
        {
            //SE DEBE VALIDAR QUE EN LIQUIDACIONES DETALLES NO ESTÉ ESTE TRANSITO
            int iLiquidaciones = _context
                .LineaLiquidaciones.Where(i => i.hist_cabecera_transito_id == id)
                .Count();
            int iLiquidacionesHistoricos = _context
                .HistLineaLiquidaciones.Where(i => i.hist_cabecera_transito_id == id)
                .Count();
            if (iLiquidaciones + iLiquidacionesHistoricos == 0)
            {
                //SE CREA EL ITEM EN LA TABLA DE PRODUCCION
                var item = new DataBase.Models.CabeceraTransito
                {
                    id_cabecera_transito = histItem.IdHistCabeceraTransito!.Value,
                    almacen_codigo = histItem.AlmacenCodigo,
                    almacen_id = histItem.AlmacenId,
                    detalle_mercancia = histItem.DetalleMercancia,
                    fecha_desembarque = histItem.FechaDesembarque,
                    fecha_documento = histItem.FechaDocumento,
                    fecha_embarque = histItem.FechaEmbarque,
                    fecha_estimada = histItem.FechaEstimada,
                    fecha_registro = histItem.FechaRegistro,
                    naviera = histItem.Naviera,
                    no_buque = histItem.NoBuque,
                    no_conocimiento_embarque = histItem.NoConocimientoEmbarque,
                    no_contenedor = histItem.NoContenedor,
                    no_documento = histItem.NoDocumento,
                    no_sello = histItem.NoSello,
                    no_serie_id = histItem.NoSerieId,
                    nombre_proveedor = histItem.NombreProveedor,
                    puerto_desembarque = histItem.PuertoDesembarque,
                    puerto_embarque = histItem.PuertoEmbarque,
                    tipo_contenedor_id = histItem.TipoContenedor_id,
                    total = histItem.Total,
                    fecha_creado = histItem.FechaCreado!.Value,
                    creado_por = histItem.CreadoPor!,
                    fecha_modificado = histItem.FechaModificado,
                    modificado_por = histItem.ModificadoPor,
                };

                using (
                    var transaction =
                        _context.Database.CurrentTransaction == null
                            ? _context.Database.BeginTransaction()
                            : _context.Database.CurrentTransaction
                )
                {
                    //PARA PODER INSERTAR EN PRODUCCION Y QUE CONSERVE EL ID, SE DEBE DESHABILITAR EL AUTONUMERICO Y DESPUES DE INSERTARLO SE VUELVE A ACTIVAR
                    _context.Database.ExecuteSqlRaw(
                        "SET IDENTITY_INSERT [Liquidacion].[CabeceraTransito] ON"
                    );
                    _context.CabeceraTransito.Add(item);
                    _context.SaveChanges();
                    _context.Database.ExecuteSqlRaw(
                        "SET IDENTITY_INSERT [Liquidacion].[CabeceraTransito] OFF"
                    );

                    //SE CREAN LOS DETALLES
                    if (histItem.Lineas != null)
                    {
                        var details = new List<DataBase.Models.LineaTransitos>();
                        foreach (var detail in histItem.Lineas)
                        {
                            details.Add(
                                new DataBase.Models.LineaTransitos
                                {
                                    cabecera_transito_id = detail.HistCabeceraTransitoId,
                                    creado_por = detail.CreadoPor!,
                                    fecha_creado = detail.FechaCreado!.Value,
                                    fecha_modificado = detail.FechaModificado,
                                    hist_cabecera_llegada_id = detail.HistCabeceraLlegadaId,
                                    id_linea_transito = detail.IdHistLineaTransito,
                                    modificado_por = detail.ModificadoPor,
                                    no_llegada = detail.NoLlegada,
                                }
                            );
                        }
                        //PARA PODER INSERTAR EN PRODUCCION Y QUE CONSERVE EL ID, SE DEBE DESHABILITAR EL AUTONUMERICO Y DESPUES DE INSERTARLO SE VUELVE A ACTIVAR
                        _context.Database.ExecuteSqlRaw(
                            "SET IDENTITY_INSERT [Liquidacion].[LineaTransitos] ON"
                        );
                        _context.LineaTransitos.AddRange(details);
                        _context.SaveChanges();
                        _context.Database.ExecuteSqlRaw(
                            "SET IDENTITY_INSERT [Liquidacion].[LineaTransitos] OFF"
                        );
                    }

                    //SE ELIMINAN LOS DETALLES DE LA TABLA HISTORICA
                    var detailsDB = _context.HistLineaTransitos.Where(i =>
                        i.hist_cabecera_transito_id == id
                    );
                    _context.HistLineaTransitos.RemoveRange(detailsDB);

                    //SE ELIMINA EL ENCABEZADO DE LA TABLA HISTORICA
                    var itemDB = _context
                        .HistCabeceraTransitos.Where(i => i.id_hist_cabecera_transito == id)
                        .First();
                    _context.HistCabeceraTransitos.Remove(itemDB);
                    _context.SaveChanges();
                    if (!existeTransaccion)
                    {
                        transaction.Commit();
                    }
                }

                //SE REFRESCA LA CACCHE TANTO PARA PRODUCCION COMO PARA HISTORICOS
                await RefrescarCache(true);
                return new Resultado { Exito = true };
            }
            else
            {
                return new Resultado
                {
                    Exito = false,
                    Mensaje = string.Format(
                        "No se puede recuperar el Tránsito {0} porque está asociado a una Liquidación",
                        id.ToString()
                    ),
                };
            }
        }
        return new Resultado
        {
            Exito = false,
            Mensaje = string.Format("No se encontró el Tránsito {0} para recuperar", id.ToString()),
        };
    }

    public async Task<bool> RefrescarCache(bool incluyeProduccion = false)
    {
        _memoryCache.Remove("HistTransitos");
        await ObtenerTransitos();
        if (incluyeProduccion)
        {
            Services.ServicioTransito service = new ServicioTransito(
                _httpContextAccessor,
                _context,
                _settingsIO,
                _httpClientFactory,
                _memoryCache,
                _servicioAutorizacion,
                null,
                null,
                this,
                null
            );
            await service.RefrescarCache();
        }
        return true;
    }
}
