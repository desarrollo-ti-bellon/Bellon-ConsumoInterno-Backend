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

public class ServicioHistLiquidacion : IServicioHistLiquidacion
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;

    private readonly IOptions<AppSettings> _settingsIO;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioHistLiquidacion(
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

    public async Task<List<HistCabeceraLiquidacion>> ObtenerLiquidaciones()
    {
        var cache = _memoryCache.Get<List<HistCabeceraLiquidacion>>("HistLiquidaciones");
        if (cache == null)
        {
            cache = _context
                .HistCabeceraLiquidaciones.Select(i => new HistCabeceraLiquidacion
                {
                    AgenteId = i.agente_id,
                    DgaLiquidacion = i.dga_liquidacion,
                    FechaDua = i.fecha_dua,
                    IdHistCabeceraLiquidacion = i.id_hist_cabecera_liquidacion,
                    MontoArticulo52 = i.monto_articulo_52,
                    MontoFlete = i.monto_flete,
                    MontoImpuesto = i.monto_impuesto,
                    MontoMulta = i.monto_multa,
                    MontoOtrosGastos = i.monto_otros_gastos,
                    MontoSeguro = i.monto_seguro,
                    NoDua = i.no_dua,
                    TasaAduana = i.tasa_aduana,
                    TasaDolar = i.tasa_dolar,
                    TotalCifGeneral = i.total_cif_general,
                    TotalGastoManejo = i.total_gasto_manejo,
                    TotalGravamenGeneral = i.total_gravamen_general,
                    TotalItbisGeneral = i.total_itbis_general,
                    TotalSelectivoGeneral = i.total_selectivo_general,
                    DetalleMercancia = i.detalle_mercancia,
                    FechaDocumento = i.fecha_documento,
                    NoConocimientoEmbarque = i.no_conocimiento_embarque,
                    NoDocumento = i.no_documento,
                    NombreProveedor = i.nombre_proveedor,
                    NoSerieId = i.no_serie_id,
                    FechaCreado = i.fecha_creado,
                    FechaModificado = i.fecha_modificado,
                    FechaRegistro = i.fecha_registro,
                    ModificadoPor = i.modificado_por,
                    CreadoPor = i.creado_por,
                })
                .ToList();
            _memoryCache.Set<List<HistCabeceraLiquidacion>>(
                "HistLiquidaciones",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return cache.OrderBy(i => i.IdHistCabeceraLiquidacion).ToList();
    }

    public async Task<HistCabeceraLiquidacion> ObtenerLiquidacion(int id)
    {
        var allItems = await ObtenerLiquidaciones();
        var item = allItems.Where(i => i.IdHistCabeceraLiquidacion == id).FirstOrDefault().Clone();
        if (item != null)
        {
            item.Lineas = _context
                .HistLineaLiquidaciones.Where(i => i.hist_cabecera_liquidacion_id == id)
                .Select(i => new HistLineaLiquidacion
                {
                    AlmacenCodigo = i.almacen_codigo,
                    AlmacenId = i.almacen_id,
                    HistCabeceraLiquidacion_id = i.hist_cabecera_liquidacion_id,
                    Cantidad = i.cantidad,
                    CodigoArancelarioId = i.codigo_arancelario_id,
                    CodigoArancelarioCod = i.codigo_arancelario.no_cod_arancelario,
                    CodigoPaisOrigen = i.codigo_pais_origen,
                    CodigoUnidadMedida = i.codigo_unidad_medida,
                    CostoProductoRd = i.costo_producto_rd,
                    CostoProductoUs = i.costo_producto_us,
                    DescripcionProducto = i.descripcion_producto,
                    FobAduanaUs = i.fob_aduana_us,
                    HistCabeceraTransitoId = i.hist_cabecera_transito_id,
                    IdHistLineaLiquidacion = i.id_hist_linea_liquidacion,
                    IdPaisOrigen = i.id_pais_origen,
                    IdProducto = i.id_producto,
                    IdUnidadMedida = i.id_unidad_medida,
                    ImporteUs = i.importe_us,
                    NoProducto = i.no_producto,
                    ReferenciaProducto = i.referencia_producto,
                    TasaLiquidacion = i.tasa_liquidacion,
                    TotalCif = i.total_cif,
                    TotalGeneral = i.total_general,
                    TotalGravamen = i.total_gravamen,
                    TotalItbis = i.total_itbis,
                    TasaItbis = i.tasa_itbis,
                    TotalSelectivo = i.total_selectivo,
                    UltimoCostoProductoRd = i.ultimo_costo_producto_rd,
                    FechaModificado = i.fecha_modificado,
                    ModificadoPor = i.modificado_por,
                    CreadoPor = i.creado_por,
                    FechaCreado = i.fecha_creado,
                })
                .OrderBy(i => i.IdHistLineaLiquidacion)
                .ToList();

            item.Cargos = _context
                .HistCargosAdicionales.Where(i => i.hist_cabecera_liquidacion_id == id)
                .Select(i => new HistCargoAdicional
                {
                    HisCabeceraLiquidacionId = i.hist_cabecera_liquidacion_id,
                    DescripcionCargoProducto = i.descripcion_cargo_producto,
                    FechaDocumento = i.fecha_documento,
                    IdHistCargoAdicional = i.id_hist_cargo_adicional,
                    IdCargoProducto = i.id_cargo_producto,
                    MontoDocumento = i.monto_documento,
                    NoCargoProducto = i.no_cargo_producto,
                    Observacion = i.observacion,
                    FechaModificado = i.fecha_modificado,
                    ModificadoPor = i.modificado_por,
                    CreadoPor = i.creado_por,
                    FechaCreado = i.fecha_creado,
                })
                .OrderBy(i => i.IdHistCargoAdicional)
                .ToList();
        }
        return item;
    }

    public async Task<Resultado> Recuperar(int id)
    {
        var histItem = await ObtenerLiquidacion(id);
        bool existeTransaccion = _context.Database.CurrentTransaction == null ? false : true;
        if (histItem != null)
        {
            //SE CREA EL ITEM EN LA TABLA DE PRODUCCION
            var item = new DataBase.Models.CabeceraLiquidaciones
            {
                id_cabecera_liquidacion = histItem.IdHistCabeceraLiquidacion,
                agente_id = histItem.AgenteId,
                detalle_mercancia = histItem.DetalleMercancia,
                dga_liquidacion = histItem.DgaLiquidacion,
                fecha_documento = histItem.FechaDocumento,
                fecha_dua = histItem.FechaDua,
                fecha_registro = histItem.FechaRegistro,
                monto_articulo_52 = histItem.MontoArticulo52,
                monto_flete = histItem.MontoFlete,
                monto_impuesto = histItem.MontoImpuesto,
                monto_multa = histItem.MontoMulta,
                monto_otros_gastos = histItem.MontoOtrosGastos,
                monto_seguro = histItem.MontoSeguro,
                no_conocimiento_embarque = histItem.NoConocimientoEmbarque,
                no_documento = histItem.NoDocumento,
                no_dua = histItem.NoDua,
                no_serie_id = histItem.NoSerieId,
                nombre_proveedor = histItem.NombreProveedor,
                tasa_aduana = histItem.TasaAduana,
                tasa_dolar = histItem.TasaDolar,
                total_cif_general = histItem.TotalCifGeneral,
                total_gasto_manejo = histItem.TotalGastoManejo,
                total_gravamen_general = histItem.TotalGravamenGeneral,
                total_itbis_general = histItem.TotalItbisGeneral,
                total_selectivo_general = histItem.TotalSelectivoGeneral,
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
                    "SET IDENTITY_INSERT [Liquidacion].[CabeceraLiquidaciones] ON"
                );
                _context.CabeceraLiquidaciones.Add(item);
                _context.SaveChanges();
                _context.Database.ExecuteSqlRaw(
                    "SET IDENTITY_INSERT [Liquidacion].[CabeceraLiquidaciones] OFF"
                );

                //SE CREAN LOS DETALLES
                if (histItem.Lineas != null)
                {
                    var details = new List<DataBase.Models.LineaLiquidaciones>();
                    foreach (var detail in histItem.Lineas)
                    {
                        details.Add(
                            new DataBase.Models.LineaLiquidaciones
                            {
                                id_linea_liquidacion = detail.IdHistLineaLiquidacion,
                                cabecera_liquidacion_id = detail.HistCabeceraLiquidacion_id,
                                hist_cabecera_transito_id = detail.HistCabeceraTransitoId,
                                almacen_codigo = detail.AlmacenCodigo,
                                almacen_id = detail.AlmacenId,
                                cantidad = detail.Cantidad,
                                codigo_arancelario_id = detail.CodigoArancelarioId,
                                codigo_pais_origen = detail.CodigoPaisOrigen,
                                codigo_unidad_medida = detail.CodigoUnidadMedida,
                                costo_producto_rd = detail.CostoProductoRd,
                                costo_producto_us = detail.CostoProductoUs,
                                descripcion_producto = detail.DescripcionProducto,
                                fob_aduana_us = detail.FobAduanaUs,
                                id_pais_origen = detail.IdPaisOrigen,
                                id_producto = detail.IdProducto,
                                id_unidad_medida = detail.IdUnidadMedida,
                                importe_us = detail.ImporteUs,
                                no_producto = detail.NoProducto,
                                referencia_producto = detail.ReferenciaProducto,
                                tasa_liquidacion = detail.TasaLiquidacion,
                                total_cif = detail.TotalCif,
                                total_general = detail.TotalGeneral,
                                total_gravamen = detail.TotalGravamen,
                                total_itbis = detail.TotalItbis,
                                tasa_itbis = detail.TasaItbis,
                                total_selectivo = detail.TotalSelectivo,
                                ultimo_costo_producto_rd = detail.UltimoCostoProductoRd,
                                creado_por = detail.CreadoPor!,
                                fecha_creado = detail.FechaCreado!.Value,
                                fecha_modificado = detail.FechaModificado,
                                modificado_por = detail.ModificadoPor,
                            }
                        );
                    }
                    //PARA PODER INSERTAR EN PRODUCCION Y QUE CONSERVE EL ID, SE DEBE DESHABILITAR EL AUTONUMERICO Y DESPUES DE INSERTARLO SE VUELVE A ACTIVAR
                    _context.Database.ExecuteSqlRaw(
                        "SET IDENTITY_INSERT [Liquidacion].[LineaLiquidaciones] ON"
                    );
                    _context.LineaLiquidaciones.AddRange(details);
                    _context.SaveChanges();
                    _context.Database.ExecuteSqlRaw(
                        "SET IDENTITY_INSERT [Liquidacion].[LineaLiquidaciones] OFF"
                    );
                }

                //SE CREAN LOS DETALLES
                if (histItem.Cargos != null)
                {
                    var detailsCargos = new List<DataBase.Models.CargosAdicionales>();
                    foreach (var detail in histItem.Cargos)
                    {
                        detailsCargos.Add(
                            new DataBase.Models.CargosAdicionales
                            {
                                id_cargo_adicional = detail.IdHistCargoAdicional,
                                cabecera_liquidacion_id = detail.HisCabeceraLiquidacionId,
                                descripcion_cargo_producto = detail.DescripcionCargoProducto,
                                fecha_documento = detail.FechaDocumento,
                                id_cargo_producto = detail.IdCargoProducto,
                                monto_documento = detail.MontoDocumento,
                                no_cargo_producto = detail.NoCargoProducto,
                                observacion = detail.Observacion,
                                creado_por = detail.CreadoPor!,
                                fecha_creado = detail.FechaCreado!.Value,
                                fecha_modificado = detail.FechaModificado,
                                modificado_por = detail.ModificadoPor,
                            }
                        );
                    }
                    //PARA PODER INSERTAR EN PRODUCCION Y QUE CONSERVE EL ID, SE DEBE DESHABILITAR EL AUTONUMERICO Y DESPUES DE INSERTARLO SE VUELVE A ACTIVAR
                    _context.Database.ExecuteSqlRaw(
                        "SET IDENTITY_INSERT [Liquidacion].[CargosAdicionales] ON"
                    );
                    _context.CargosAdicionales.AddRange(detailsCargos);
                    _context.SaveChanges();
                    _context.Database.ExecuteSqlRaw(
                        "SET IDENTITY_INSERT [Liquidacion].[CargosAdicionales] OFF"
                    );
                }

                //SE ELIMINAN LOS DETALLES DE LA TABLA HISTORICA
                var detailsDB = _context.HistLineaLiquidaciones.Where(i =>
                    i.hist_cabecera_liquidacion_id == id
                );
                _context.HistLineaLiquidaciones.RemoveRange(detailsDB);

                //SE ELIMINAN LOS DETALLES DE LA TABLA HISTORICA
                var detailsCargosDB = _context.HistCargosAdicionales.Where(i =>
                    i.hist_cabecera_liquidacion_id == id
                );
                _context.HistCargosAdicionales.RemoveRange(detailsCargosDB);

                //SE ELIMINA EL ENCABEZADO DE LA TABLA HISTORICA
                var itemDB = _context
                    .HistCabeceraLiquidaciones.Where(i => i.id_hist_cabecera_liquidacion == id)
                    .First();
                _context.HistCabeceraLiquidaciones.Remove(itemDB);
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
        return new Resultado
        {
            Exito = false,
            Mensaje = string.Format(
                "No se encontró la Liquidación {0} para recuperar",
                id.ToString()
            ),
        };
    }

    public async Task<bool> RefrescarCache(bool incluyeProduccion = false)
    {
        _memoryCache.Remove("HistLiquidaciones");
        await ObtenerLiquidaciones();
        if (incluyeProduccion)
        {
            Services.ServicioLiquidacion service = new ServicioLiquidacion(
                _httpContextAccessor,
                _context,
                _settingsIO,
                _httpClientFactory,
                _memoryCache,
                _servicioAutorizacion,
                null,
                null,
                this,
                null,
                null
            );
            await service.RefrescarCache();
        }
        return true;
    }
}
