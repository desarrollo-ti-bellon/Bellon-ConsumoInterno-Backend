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

public class ServicioHistLlegada : IServicioHistLlegada
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;

    private readonly IOptions<AppSettings> _settingsIO;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioHistLlegada(
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

    public async Task<List<HistCabeceraLlegada>> ObtenerLlegadas()
    {
        var cache = _memoryCache.Get<List<HistCabeceraLlegada>>("HistLlegadas");
        if (cache == null)
        {
            cache = _context
                .HistCabeceraLlegadas.Select(i => new HistCabeceraLlegada
                {
                    IdHistCabeceraLlegada = i.id_hist_cabecera_llegada,
                    NoDocumento = i.no_documento,
                    FechaDocumento = i.fecha_documento,
                    NoDocumentoOrigen = i.no_documento_origen,
                    NoFacturaProveedor = i.no_factura_proveedor,
                    NoProveedor = i.no_proveedor,
                    NombreProveedor = i.nombre_proveedor,
                    NoAlmacenUS = i.no_almacen_us,
                    Total = i.total,
                    NombreAgente = i.agente.nombre,
                    CreadoPor = i.creado_por,
                    AgenteId = i.agente_id,
                    CantidadPeso = i.cantidad_peso,
                    CantidadPieza = i.cantidad_pieza,
                    CantidadVolumen = i.cantidad_volumen,
                    CodigoPeso = i.codigo_peso,
                    CodigoVolumen = i.codigo_volumen,
                    FechaCreado = i.fecha_creado,
                    FechaModificado = i.fecha_modificado,
                    FechaRegistro = i.fecha_registro,
                    IdDocumentoOrigen = i.id_documento_origen,
                    IdPeso = i.id_peso,
                    IdProveedor = i.id_proveedor,
                    IdVolumen = i.id_volumen,
                    ModificadoPor = i.modificado_por,
                    NoDocumentoPrevio = i.no_documento_previo,
                    NoSerieId = i.no_serie_id,
                    Transportista = i.transportista,
                    CantidadLineas = i.HistLineaLlegadas.Count,
                })
                .ToList();
            _memoryCache.Set<List<HistCabeceraLlegada>>(
                "HistLlegadas",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return cache.OrderBy(i => i.IdHistCabeceraLlegada.Value).ToList();
        ;
    }

    public async Task<HistCabeceraLlegada> ObtenerLlegada(int id)
    {
        var allItems = await ObtenerLlegadas();
        var item = allItems.Where(i => i.IdHistCabeceraLlegada == id).FirstOrDefault().Clone();
        if (item != null)
        {
            item.Lineas = _context
                .HistLineaLlegadas.Where(i => i.hist_cabecera_llegada_id == id)
                .Select(i => new HistLineaLlegada
                {
                    AlmacenCodigo = i.almacen_codigo,
                    AlmacenId = i.almacen_id,
                    HistCabeceraLlegadaId = i.hist_cabecera_llegada_id,
                    Cantidad = i.cantidad,
                    CantidadOrigen = i.cantidad_origen,
                    CodigoPaisOrigen = i.codigo_pais_origen,
                    CodigoUnidadMedida = i.codigo_unidad_medida,
                    CostoUnitario = i.costo_unitario,
                    CostoUnitarioDirecto = i.costo_unitario_directo,
                    CreadoPor = i.creado_por,
                    DescripcionProducto = i.descripcion_producto,
                    FechaCreado = i.fecha_creado,
                    IdHistLineaLlegada = i.id_hist_linea_llegada,
                    IdPais = i.id_pais,
                    IdProducto = i.id_producto,
                    IdUnidadMedida = i.id_unidad_medida,
                    NoProducto = i.no_producto,
                    PrecioUnitario = i.precio_unitario,
                    ReferenciaProducto = i.referencia_producto,
                    Total = i.total,
                    IdCodArancelario = i.id_cod_arancelario,
                    NoCodArancelario = i.no_cod_arancelario,
                })
                .OrderBy(i => i.IdHistLineaLlegada)
                .ToList();
        }
        return item;
    }

    public async Task<Resultado> Recuperar(int id)
    {
        var histItem = await ObtenerLlegada(id);
        bool existeTransaccion = _context.Database.CurrentTransaction == null ? false : true;
        if (histItem != null)
        {
            //SE DEBE VALIDAR QUE EN TRANSITOS DETALLES NO ESTÉ ESTA LLEGADA
            int iTransitos = _context
                .LineaTransitos.Where(i => i.hist_cabecera_llegada_id == id)
                .Count();
            int iTransitosHistoricos = _context
                .HistLineaTransitos.Where(i => i.hist_cabecera_llegada_id == id)
                .Count();
            if (iTransitos + iTransitosHistoricos == 0)
            {
                //SE CREA EL ITEM EN LA TABLA DE PRODUCCION
                var item = new DataBase.Models.CabeceraLlegadas
                {
                    id_cabecera_llegada = histItem.IdHistCabeceraLlegada!.Value,
                    agente_id = histItem.AgenteId,
                    cantidad_peso = histItem.CantidadPeso,
                    cantidad_pieza = histItem.CantidadPieza,
                    cantidad_volumen = histItem.CantidadVolumen,
                    codigo_peso = histItem.CodigoPeso,
                    codigo_volumen = histItem.CodigoVolumen,
                    fecha_documento = histItem.FechaDocumento,
                    fecha_registro = histItem.FechaRegistro,
                    id_documento_origen = histItem.IdDocumentoOrigen,
                    id_peso = histItem.IdPeso,
                    id_proveedor = histItem.IdProveedor,
                    id_volumen = histItem.IdVolumen,
                    no_almacen_us = histItem.NoAlmacenUS,
                    no_documento = histItem.NoDocumento,
                    no_documento_origen = histItem.NoDocumentoOrigen,
                    no_documento_previo = histItem.NoDocumentoPrevio,
                    no_factura_proveedor = histItem.NoFacturaProveedor,
                    no_proveedor = histItem.NoProveedor,
                    no_serie_id = histItem.NoSerieId,
                    nombre_proveedor = histItem.NombreProveedor,
                    total = histItem.Total,
                    transportista = histItem.Transportista,
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
                        "SET IDENTITY_INSERT [Liquidacion].[CabeceraLlegadas] ON"
                    );
                    _context.CabeceraLlegadas.Add(item);
                    _context.SaveChanges();
                    _context.Database.ExecuteSqlRaw(
                        "SET IDENTITY_INSERT [Liquidacion].[CabeceraLlegadas] OFF"
                    );

                    //SE CREAN LOS DETALLES
                    if (histItem.Lineas != null)
                    {
                        var details = new List<DataBase.Models.LineaLlegadas>();
                        foreach (var detail in histItem.Lineas)
                        {
                            details.Add(
                                new DataBase.Models.LineaLlegadas
                                {
                                    id_linea_llegada = detail.IdHistLineaLlegada,
                                    cabecera_llegada_id = detail.HistCabeceraLlegadaId,
                                    almacen_codigo = detail.AlmacenCodigo,
                                    almacen_id = detail.AlmacenId,
                                    cantidad = detail.Cantidad,
                                    cantidad_origen = detail.CantidadOrigen,
                                    codigo_pais_origen = detail.CodigoPaisOrigen,
                                    codigo_unidad_medida = detail.CodigoUnidadMedida,
                                    costo_unitario = detail.CostoUnitario,
                                    costo_unitario_directo = detail.CostoUnitarioDirecto,
                                    creado_por = detail.CreadoPor!,
                                    descripcion_producto = detail.DescripcionProducto,
                                    fecha_creado = detail.FechaCreado!.Value,
                                    id_pais = detail.IdPais,
                                    id_producto = detail.IdProducto,
                                    id_unidad_medida = detail.IdUnidadMedida,
                                    no_producto = detail.NoProducto,
                                    precio_unitario = detail.PrecioUnitario,
                                    referencia_producto = detail.ReferenciaProducto,
                                    total = detail.Total,
                                    id_cod_arancelario = detail.IdCodArancelario,
                                    no_cod_arancelario = detail.NoCodArancelario,
                                    fecha_modificado = detail.FechaModificado,
                                    modificado_por = detail.ModificadoPor,
                                }
                            );
                        }
                        //PARA PODER INSERTAR EN PRODUCCION Y QUE CONSERVE EL ID, SE DEBE DESHABILITAR EL AUTONUMERICO Y DESPUES DE INSERTARLO SE VUELVE A ACTIVAR
                        _context.Database.ExecuteSqlRaw(
                            "SET IDENTITY_INSERT [Liquidacion].[LineaLlegadas] ON"
                        );
                        _context.LineaLlegadas.AddRange(details);
                        _context.SaveChanges();
                        _context.Database.ExecuteSqlRaw(
                            "SET IDENTITY_INSERT [Liquidacion].[LineaLlegadas] OFF"
                        );
                    }

                    //SE ELIMINAN LOS DETALLES DE LA TABLA HISTORICA
                    var detailsDB = _context.HistLineaLlegadas.Where(i =>
                        i.hist_cabecera_llegada_id == id
                    );
                    _context.HistLineaLlegadas.RemoveRange(detailsDB);

                    //SE ELIMINA EL ENCABEZADO DE LA TABLA HISTORICA
                    var itemDB = _context
                        .HistCabeceraLlegadas.Where(i => i.id_hist_cabecera_llegada == id)
                        .First();
                    _context.HistCabeceraLlegadas.Remove(itemDB);
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
                        "No se puede recuperar la llegada {0} porque está asociada a un Tránsito",
                        id.ToString()
                    ),
                };
            }
        }
        return new Resultado
        {
            Exito = false,
            Mensaje = string.Format("No se encontró la llegada {0} para recuperar", id.ToString()),
        };
    }

    public async Task<bool> RefrescarCache(bool incluyeProduccion = false)
    {
        _memoryCache.Remove("HistLlegadas");
        await ObtenerLlegadas();
        if (incluyeProduccion)
        {
            Services.ServicioLlegada service = new ServicioLlegada(
                _httpContextAccessor,
                _context,
                _settingsIO,
                _httpClientFactory,
                _memoryCache,
                _servicioAutorizacion,
                this,
                null
            );
            await service.RefrescarCache();
        }
        return true;
    }
}
