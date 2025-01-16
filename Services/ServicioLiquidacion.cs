using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Bellon.API.Liquidacion.Classes;
using Bellon.API.Liquidacion.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Bellon.API.Liquidacion.Services;

public class ServicioLiquidacion : IServicioLiquidacion
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;
    private readonly IServicioTransito _servicioTransito;
    private readonly IServicioHistTransito _servicioHistTransito;
    private readonly IServicioHistLiquidacion _servicioHistLiquidacion;
    private readonly IServicioNumeroSerie _servicioNumeroSerie;
    private readonly IServicioOrdenTransferencia _servicioOrdenTransferencia;

    public ServicioLiquidacion(
        IHttpContextAccessor httpContextAccessor,
        DataBase.AppDataBase context,
        IOptions<AppSettings> settings,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IServicioAutorizacion servicioAutorizacion,
        IServicioTransito servicioTransito,
        IServicioHistTransito servicioHistTransito,
        IServicioHistLiquidacion servicioHistLiquidacion,
        IServicioNumeroSerie servicioNumeroSerie,
        IServicioOrdenTransferencia servicioOrdenTransferencia
    )
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _servicioAutorizacion = servicioAutorizacion;
        _servicioTransito = servicioTransito;
        _servicioHistTransito = servicioHistTransito;
        _servicioHistLiquidacion = servicioHistLiquidacion;
        _servicioNumeroSerie = servicioNumeroSerie;
        _servicioOrdenTransferencia = servicioOrdenTransferencia;
    }

    public async Task<List<CabeceraLiquidacion>> ObtenerLiquidaciones()
    {
        var cache = _memoryCache.Get<List<CabeceraLiquidacion>>("Liquidaciones");
        if (cache == null)
        {
            cache = _context
                .CabeceraLiquidaciones.Select(i => new CabeceraLiquidacion
                {
                    AgenteId = i.agente_id,
                    DgaLiquidacion = i.dga_liquidacion,
                    FechaDua = i.fecha_dua,
                    IdCabeceraLiquidacion = i.id_cabecera_liquidacion,
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
            _memoryCache.Set<List<CabeceraLiquidacion>>(
                "Liquidaciones",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return cache.OrderBy(i => i.IdCabeceraLiquidacion.Value).ToList();
    }

    public async Task<CabeceraLiquidacion> ObtenerLiquidacion(int id)
    {
        var allItems = await ObtenerLiquidaciones();
        var item = allItems.Where(i => i.IdCabeceraLiquidacion == id).FirstOrDefault().Clone();
        if (item != null)
        {
            item.Lineas = _context
                .LineaLiquidaciones.Where(i => i.cabecera_liquidacion_id == id)
                .Select(i => new LineaLiquidacion
                {
                    AlmacenCodigo = i.almacen_codigo,
                    AlmacenId = i.almacen_id,
                    CabeceraLiquidacionId = i.cabecera_liquidacion_id,
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
                    IdLineaLiquidacion = i.id_linea_liquidacion,
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
                .OrderBy(i => i.IdLineaLiquidacion)
                .ToList();

            item.Cargos = _context
                .CargosAdicionales.Where(i => i.cabecera_liquidacion_id == id)
                .Select(i => new CargoAdicional
                {
                    CabeceraLiquidacionId = i.cabecera_liquidacion_id,
                    DescripcionCargoProducto = i.descripcion_cargo_producto,
                    FechaDocumento = i.fecha_documento,
                    IdCargoAdicional = i.id_cargo_adicional,
                    IdCargoProducto = i.id_cargo_producto,
                    MontoDocumento = i.monto_documento,
                    NoCargoProducto = i.no_cargo_producto,
                    Observacion = i.observacion,
                    FechaModificado = i.fecha_modificado,
                    ModificadoPor = i.modificado_por,
                    CreadoPor = i.creado_por,
                    FechaCreado = i.fecha_creado,
                })
                .OrderBy(i => i.IdCargoAdicional)
                .ToList();
        }
        return item;
    }

    public async Task<CabeceraLiquidacion> GuardarCabeceraLiquidacion(CabeceraLiquidacion item)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (item.IdCabeceraLiquidacion.HasValue)
        {
            var oldItem = _context
                .CabeceraLiquidaciones.Where(i =>
                    i.id_cabecera_liquidacion == item.IdCabeceraLiquidacion.Value
                )
                .FirstOrDefault();
            if (oldItem != null)
            {
                //ACTUALIZA EL OBJETO EXISTENTE
                oldItem.agente_id = item.AgenteId;
                oldItem.detalle_mercancia = item.DetalleMercancia;
                oldItem.dga_liquidacion = item.DgaLiquidacion;
                oldItem.fecha_documento = item.FechaDocumento;
                oldItem.fecha_dua = item.FechaDua;
                oldItem.monto_articulo_52 = item.MontoArticulo52;
                oldItem.monto_flete = item.MontoFlete;
                oldItem.monto_impuesto = item.MontoImpuesto;
                oldItem.monto_multa = item.MontoMulta;
                oldItem.monto_otros_gastos = item.MontoOtrosGastos;
                oldItem.monto_seguro = item.MontoSeguro;
                oldItem.no_conocimiento_embarque = item.NoConocimientoEmbarque;
                oldItem.no_dua = item.NoDua;
                oldItem.nombre_proveedor = item.NombreProveedor;
                oldItem.tasa_aduana = item.TasaAduana;
                oldItem.tasa_dolar = item.TasaDolar;
                oldItem.total_cif_general = item.TotalCifGeneral;
                oldItem.total_gasto_manejo = item.TotalGastoManejo;
                oldItem.total_gravamen_general = item.TotalGravamenGeneral;
                oldItem.total_itbis_general = item.TotalItbisGeneral;
                oldItem.total_selectivo_general = item.TotalSelectivoGeneral;
                oldItem.fecha_modificado = DateTime.UtcNow;
                oldItem.modificado_por = identity!.Name;
                try
                {
                    _context.SaveChanges();
                    CalcularTotalesLineas(oldItem);

                    //SE ACTUALIZAN LOS TOTALES DE LA CABECERA DESPUES DE HABER CALCULADO LAS LINEAS
                    CalcularTotalesCabecera(oldItem.id_cabecera_liquidacion);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al actualizar el registro: <" + ex.Message + ">");
                }

                //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
                await RefrescarCache();

                //SE RETORNA EL OBJETO MODIFICADO
                return await ObtenerLiquidacion(oldItem.id_cabecera_liquidacion);
            }
        }
        else
        {
            //SE INSERTA EL NUEVO ITEM
            var numeroSerie = await _servicioNumeroSerie.ObtenerNumeroDocumento(
                _settings.DocumentoLiquidacionNoSerieId
            );
            var newItemData = new DataBase.Models.CabeceraLiquidaciones
            {
                agente_id = item.AgenteId,
                detalle_mercancia = item.DetalleMercancia,
                dga_liquidacion = item.DgaLiquidacion,
                fecha_documento = item.FechaDocumento,
                fecha_dua = item.FechaDua,
                fecha_registro = item.FechaRegistro,
                monto_articulo_52 = item.MontoArticulo52,
                monto_flete = item.MontoFlete,
                monto_impuesto = item.MontoImpuesto,
                monto_multa = item.MontoMulta,
                monto_otros_gastos = item.MontoOtrosGastos,
                monto_seguro = item.MontoSeguro,
                no_conocimiento_embarque = item.NoConocimientoEmbarque,
                no_documento = numeroSerie,
                no_serie_id = _settings.DocumentoLiquidacionNoSerieId,
                no_dua = item.NoDua,
                nombre_proveedor = item.NombreProveedor,
                tasa_aduana = item.TasaAduana,
                tasa_dolar = item.TasaDolar,
                total_cif_general = item.TotalCifGeneral,
                total_gasto_manejo = item.TotalGastoManejo,
                total_gravamen_general = item.TotalGravamenGeneral,
                total_itbis_general = item.TotalItbisGeneral,
                total_selectivo_general = item.TotalSelectivoGeneral,
                fecha_creado = DateTime.UtcNow,
                creado_por = identity!.Name!,
            };
            var newItem = _context.CabeceraLiquidaciones.Add(newItemData);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear el registro: <" + ex.Message + ">");
            }

            //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
            await RefrescarCache();

            //SE RETORNA EL OBJETO CREADO
            return await ObtenerLiquidacion(newItem.Entity.id_cabecera_liquidacion);
        }
        return null;
    }

    public async Task<CabeceraLiquidacion> GuardarLineaLiquidacion(List<LineaLiquidacion> items)
    {
        if (items.Count > 0)
        {
            var parent = _context
                .CabeceraLiquidaciones.Where(i =>
                    i.id_cabecera_liquidacion == items[0].CabeceraLiquidacionId
                )
                .FirstOrDefault();
            if (parent != null)
            {
                var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                foreach (var item in items)
                {
                    if (item.HistCabeceraTransitoId > 0)
                    {
                        if (
                            ExisteTransitoEnOtraLiquidacion(
                                item.HistCabeceraTransitoId,
                                item.CabeceraLiquidacionId
                            )
                        )
                        {
                            throw new Exception(
                                string.Format(
                                    "El Tránsito {0} se encuentra en otra Liquidación, por favor verifique",
                                    item.HistCabeceraTransitoId
                                )
                            );
                        }

                        if (item.IdLineaLiquidacion.HasValue)
                        {
                            var oldItem = _context
                                .LineaLiquidaciones.Where(i =>
                                    i.id_linea_liquidacion == item.IdLineaLiquidacion.Value
                                    && i.cabecera_liquidacion_id == item.CabeceraLiquidacionId
                                )
                                .FirstOrDefault();
                            if (oldItem != null)
                            {
                                var codigoArancelario = _context.CodigoArancelarios.FirstOrDefault(
                                    c =>
                                        c.id_codigo_arancelario == item.CodigoArancelarioId
                                        && c.id_pais == item.IdPaisOrigen
                                );

                                if (codigoArancelario != null)
                                {
                                    oldItem.almacen_codigo = item.AlmacenCodigo;
                                    oldItem.almacen_id = item.AlmacenId;
                                    oldItem.cantidad = item.Cantidad;
                                    oldItem.codigo_arancelario_id = item.CodigoArancelarioId;
                                    oldItem.codigo_pais_origen = item.CodigoPaisOrigen;
                                    oldItem.codigo_unidad_medida = item.CodigoUnidadMedida;
                                    oldItem.costo_producto_us = item.CostoProductoUs;
                                    oldItem.descripcion_producto = item.DescripcionProducto;
                                    oldItem.fob_aduana_us = item.FobAduanaUs;
                                    oldItem.id_pais_origen = item.IdPaisOrigen;
                                    oldItem.id_producto = item.IdProducto;
                                    oldItem.id_unidad_medida = item.IdUnidadMedida;
                                    oldItem.importe_us = item.CostoProductoUs * item.Cantidad;
                                    oldItem.no_producto = item.NoProducto;
                                    oldItem.referencia_producto = item.ReferenciaProducto;
                                    oldItem.tasa_itbis = item.TasaItbis;
                                    oldItem.ultimo_costo_producto_rd = item.UltimoCostoProductoRd;

                                    var otrasLineas = _context.LineaLiquidaciones.Where(i =>
                                        i.cabecera_liquidacion_id == parent.id_cabecera_liquidacion
                                        && i.id_linea_liquidacion != oldItem.id_linea_liquidacion
                                    );

                                    oldItem.total_cif = CalcularLineaTotalCif(
                                        oldItem.fob_aduana_us,
                                        oldItem.cantidad,
                                        parent.monto_seguro,
                                        parent.monto_flete,
                                        parent.monto_otros_gastos,
                                        parent.tasa_aduana,
                                        otrasLineas.Sum(i => i.fob_aduana_us * i.cantidad)
                                    );
                                    oldItem.total_gravamen = CalcularLineaTotalGravamen(
                                        oldItem.total_cif,
                                        codigoArancelario.porciento_gravamen
                                    );
                                    oldItem.total_selectivo = CalcularLineaTotalSelectivo(
                                        oldItem.total_cif,
                                        codigoArancelario.porciento_selectivo,
                                        oldItem.total_gravamen
                                    );
                                    oldItem.total_itbis = CalcularLineaTotalItbis(
                                        oldItem.total_cif,
                                        oldItem.total_gravamen,
                                        oldItem.total_selectivo,
                                        oldItem.tasa_itbis
                                    );
                                    oldItem.total_general = CalcularLineaTotalGeneral(
                                        oldItem.total_gravamen,
                                        oldItem.total_selectivo,
                                        oldItem.total_itbis
                                    );
                                    oldItem.costo_producto_rd = CalcularLineaCostoRD(
                                        oldItem.costo_producto_us,
                                        oldItem.cantidad,
                                        parent.tasa_dolar,
                                        oldItem.total_gravamen,
                                        oldItem.total_selectivo,
                                        parent.total_gasto_manejo,
                                        otrasLineas.Sum(i => i.costo_producto_us * i.cantidad)
                                    );
                                    oldItem.tasa_liquidacion = CalcularLineaTasaLiquidacion(
                                        oldItem.costo_producto_rd,
                                        oldItem.costo_producto_us
                                    );
                                    oldItem.modificado_por = identity!.Name!;
                                    oldItem.fecha_modificado = DateTime.UtcNow;
                                    try
                                    {
                                        _context.SaveChanges();
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(
                                            "Error al actualizar el registro: <" + ex.Message + ">"
                                        );
                                    }
                                }
                                else
                                {
                                    throw new Exception(
                                        string.Format(
                                            "No se encontró el código arancelario {0} para el país {1}",
                                            item.CodigoArancelarioId,
                                            item.IdPaisOrigen
                                        )
                                    );
                                }
                            }
                            else
                            {
                                throw new Exception(
                                    string.Format(
                                        "No se encontró la línea {0} de la Liquidación {1}",
                                        item.IdLineaLiquidacion.Value,
                                        item.CabeceraLiquidacionId
                                    )
                                );
                            }
                        }
                        else
                        {
                            //VALIDA QUE EL NUEVO TRANSITO (HistCabeceraTransitoId) EXISTA EN PRODUCCION
                            var transito = _context
                                .CabeceraTransito.Where(i =>
                                    i.id_cabecera_transito == item.HistCabeceraTransitoId
                                )
                                .FirstOrDefault();

                            //VALIDA QUE EL NUEVO TRANSITO (HistCabeceraTransitoId) EXISTA EN HISTORICO
                            var transitoHistorico = _context
                                .HistCabeceraTransitos.Where(i =>
                                    i.id_hist_cabecera_transito == item.HistCabeceraTransitoId
                                )
                                .FirstOrDefault();

                            if (transito != null)
                            {
                                //ARCHIVA (DESACTIVA) EL TRANSITO NUEVO
                                var resultArchivar = await _servicioTransito.Archivar(
                                    item.HistCabeceraTransitoId
                                );
                                if (!resultArchivar.Exito)
                                {
                                    throw new Exception(resultArchivar.Mensaje);
                                }

                                //SE INSERTA EL NUEVO ITEM CON TOTALES EN 0
                                var newItemData = new DataBase.Models.LineaLiquidaciones
                                {
                                    cabecera_liquidacion_id = item.CabeceraLiquidacionId,
                                    hist_cabecera_transito_id = item.HistCabeceraTransitoId,
                                    almacen_codigo = item.AlmacenCodigo,
                                    almacen_id = item.AlmacenId,
                                    cantidad = item.Cantidad,
                                    codigo_arancelario_id = item.CodigoArancelarioId,
                                    codigo_pais_origen = item.CodigoPaisOrigen,
                                    codigo_unidad_medida = item.CodigoUnidadMedida,
                                    costo_producto_us = item.CostoProductoUs,
                                    descripcion_producto = item.DescripcionProducto,
                                    fob_aduana_us = item.FobAduanaUs,
                                    id_pais_origen = item.IdPaisOrigen,
                                    id_producto = item.IdProducto,
                                    id_unidad_medida = item.IdUnidadMedida,
                                    importe_us = item.CostoProductoUs * item.Cantidad,
                                    no_producto = item.NoProducto,
                                    referencia_producto = item.ReferenciaProducto,
                                    tasa_itbis = item.TasaItbis,
                                    total_cif = item.TotalCif,
                                    total_general = item.TotalGeneral,
                                    total_gravamen = item.TotalGravamen,
                                    total_itbis = item.TotalItbis,
                                    total_selectivo = item.TotalSelectivo,
                                    costo_producto_rd = item.CostoProductoRd,
                                    tasa_liquidacion = item.TasaLiquidacion,
                                    ultimo_costo_producto_rd = item.UltimoCostoProductoRd,
                                    fecha_creado = DateTime.UtcNow,
                                    creado_por = identity!.Name!,
                                };
                                var newItem = _context.LineaLiquidaciones.Add(newItemData);
                                try
                                {
                                    _context.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(
                                        "Error al crear el registro: <" + ex.Message + ">"
                                    );
                                }
                            }
                            else if (transitoHistorico != null)
                            {
                                //NO HAY NECESIDAD DE MOVERLO AL HISTORICO PORQUE YA ESTA AHÍ
                                //SE INSERTA EL NUEVO ITEM
                                var newItemData = new DataBase.Models.LineaLiquidaciones
                                {
                                    cabecera_liquidacion_id = item.CabeceraLiquidacionId,
                                    hist_cabecera_transito_id = item.HistCabeceraTransitoId,
                                    almacen_codigo = item.AlmacenCodigo,
                                    almacen_id = item.AlmacenId,
                                    cantidad = item.Cantidad,
                                    codigo_arancelario_id = item.CodigoArancelarioId,
                                    codigo_pais_origen = item.CodigoPaisOrigen,
                                    codigo_unidad_medida = item.CodigoUnidadMedida,
                                    costo_producto_rd = item.CostoProductoRd,
                                    costo_producto_us = item.CostoProductoUs,
                                    descripcion_producto = item.DescripcionProducto,
                                    fob_aduana_us = item.FobAduanaUs,
                                    id_pais_origen = item.IdPaisOrigen,
                                    id_producto = item.IdProducto,
                                    id_unidad_medida = item.IdUnidadMedida,
                                    importe_us = item.CostoProductoUs * item.Cantidad,
                                    no_producto = item.NoProducto,
                                    referencia_producto = item.ReferenciaProducto,
                                    tasa_liquidacion = item.TasaLiquidacion,
                                    total_cif = item.TotalCif,
                                    total_general = item.TotalGeneral,
                                    total_gravamen = item.TotalGravamen,
                                    total_itbis = item.TotalItbis,
                                    tasa_itbis = item.TasaItbis,
                                    total_selectivo = item.TotalSelectivo,
                                    ultimo_costo_producto_rd = item.UltimoCostoProductoRd,
                                    fecha_creado = DateTime.UtcNow,
                                    creado_por = identity!.Name!,
                                };
                                var newItem = _context.LineaLiquidaciones.Add(newItemData);
                                try
                                {
                                    _context.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(
                                        "Error al crear el registro: <" + ex.Message + ">"
                                    );
                                }
                            }
                            else
                            {
                                throw new Exception(
                                    string.Format(
                                        "No se encontró el Tránsito {0}",
                                        item.HistCabeceraTransitoId
                                    )
                                );
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("El campo hist_cabecera_transito_id es obligatorio");
                    }
                }
                //LOS ITEMS QUE SE INSERTARON SON NUEVOS, Y SE DEBE CORRER EL CALCULO DE LAS LINEAS
                if (!items[0].IdLineaLiquidacion.HasValue || items[0].IdLineaLiquidacion == 0)
                {
                    CalcularTotalesLineas(parent);
                }

                //SE CALCULAN LOS TOTALES DE LA CABECERA
                CalcularTotalesCabecera(parent.id_cabecera_liquidacion);

                //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
                await RefrescarCache();

                //SE RETORNA EL OBJETO MODIFICADO
                return await ObtenerLiquidacion(items[0].CabeceraLiquidacionId);
            }
            else
            {
                throw new Exception(
                    string.Format(
                        "No se encontró la Liquidación {0}",
                        items[0].CabeceraLiquidacionId
                    )
                );
            }
        }
        else
        {
            throw new Exception("No hay items para insertar");
        }
    }

    public async Task<CabeceraLiquidacion> GuardarCargoAdicionalLiquidacion(
        List<CargoAdicional> items
    )
    {
        if (items.Count > 0)
        {
            var parent = _context
                .CabeceraLiquidaciones.Where(i =>
                    i.id_cabecera_liquidacion == items[0].CabeceraLiquidacionId
                )
                .FirstOrDefault();
            if (parent != null)
            {
                var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                foreach (var item in items)
                {
                    if (item.IdCargoAdicional.HasValue)
                    {
                        var oldItem = _context
                            .CargosAdicionales.Where(i =>
                                i.id_cargo_adicional == item.IdCargoAdicional.Value
                                && i.cabecera_liquidacion_id == item.CabeceraLiquidacionId
                            )
                            .FirstOrDefault();
                        if (oldItem != null)
                        {
                            //ACTUALIZA EL OBJETO EXISTENTE
                            oldItem.descripcion_cargo_producto = item.DescripcionCargoProducto;
                            oldItem.fecha_documento = item.FechaDocumento;
                            oldItem.id_cargo_producto = item.IdCargoProducto;
                            oldItem.monto_documento = item.MontoDocumento;
                            oldItem.no_cargo_producto = item.NoCargoProducto;
                            oldItem.observacion = item.Observacion;
                            oldItem.fecha_modificado = DateTime.UtcNow;
                            oldItem.modificado_por = identity!.Name;
                            try
                            {
                                _context.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(
                                    "Error al actualizar el registro: <" + ex.Message + ">"
                                );
                            }
                        }
                    }
                    else
                    {
                        //SE INSERTA EL NUEVO ITEM CON SUS DETALLES
                        var newItemData = new DataBase.Models.CargosAdicionales
                        {
                            cabecera_liquidacion_id = item.CabeceraLiquidacionId,
                            descripcion_cargo_producto = item.DescripcionCargoProducto,
                            fecha_documento = item.FechaDocumento,
                            id_cargo_producto = item.IdCargoProducto,
                            monto_documento = item.MontoDocumento,
                            no_cargo_producto = item.NoCargoProducto,
                            observacion = item.Observacion,
                            fecha_creado = DateTime.UtcNow,
                            creado_por = identity!.Name!,
                        };
                        var newItem = _context.CargosAdicionales.Add(newItemData);
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
                CalcularTotalesCabeceraCargosAdicionales(parent.id_cabecera_liquidacion);

                //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
                await RefrescarCache();

                //SE RETORNA EL OBJETO CREADO
                return await ObtenerLiquidacion(items[0].CabeceraLiquidacionId);
            }
            else
            {
                throw new Exception(
                    string.Format(
                        "No se encontró la Liquidación {0}",
                        items[0].CabeceraLiquidacionId
                    )
                );
            }
        }
        else
        {
            throw new Exception("No hay items para insertar");
        }
    }

    public async Task<CabeceraLiquidacion> EliminarLineaLiquidacion(int idTransito)
    {
        var ordenes = _context
            .OrdenesTransferencia.Where(i => i.hist_cabecera_transito_id == idTransito)
            .Count();
        if (ordenes == 0)
        {
            //ELIMINA TODAS LAS LINEAS QUE TENGAN EL TRANSITO (item)
            var todasLineas = _context
                .LineaLiquidaciones.Where(i => i.hist_cabecera_transito_id == idTransito)
                .ToList();
            if (todasLineas != null)
            {
                var idLiquidacion = todasLineas[0].cabecera_liquidacion_id;
                var parent = _context
                    .CabeceraLiquidaciones.Where(i => i.id_cabecera_liquidacion == idLiquidacion)
                    .First();
                _context.LineaLiquidaciones.RemoveRange(todasLineas);
                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al eliminar el registro: <" + ex.Message + ">");
                }

                //REGRESA A PRODUCCION (ACTIVA)EL TRANSITO
                var resultActivar = await _servicioHistTransito.Recuperar(idTransito);
                if (!resultActivar.Exito)
                {
                    throw new Exception(resultActivar.Mensaje);
                }

                CalcularTotalesLineas(parent);

                CalcularTotalesCabecera(parent.id_cabecera_liquidacion);

                //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
                await RefrescarCache();

                return await ObtenerLiquidacion(idLiquidacion);
            }
            return null;
        }
        else
        {
            throw new Exception(
                "No se puede eliminar el Tránsito porque ya tiene una Orden de Transferencia"
            );
        }
    }

    public async Task<CabeceraLiquidacion> EliminarCargoAdicionalLiquidacion(int item)
    {
        var oldItem = _context
            .CargosAdicionales.Where(i => i.id_cargo_adicional == item)
            .FirstOrDefault();
        if (oldItem != null)
        {
            _context.CargosAdicionales.Remove(oldItem);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el registro: <" + ex.Message + ">");
            }

            CalcularTotalesCabeceraCargosAdicionales(oldItem.cabecera_liquidacion_id);

            //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
            await RefrescarCache();

            return await ObtenerLiquidacion(oldItem.cabecera_liquidacion_id);
        }
        return null;
    }

    public async Task<int> ObtenerActivas()
    {
        return (await ObtenerLiquidaciones()).Count;
    }

    public async Task<Resultado> Archivar(int id)
    {
        var item = await ObtenerLiquidacion(id);
        if (item != null)
        {
            //SE CREA EL ITEM EN LA TABLA DE HISTORICO
            var histItem = new DataBase.Models.HistCabeceraLiquidaciones
            {
                id_hist_cabecera_liquidacion = item.IdCabeceraLiquidacion!.Value,
                agente_id = item.AgenteId,
                detalle_mercancia = item.DetalleMercancia,
                dga_liquidacion = item.DgaLiquidacion,
                fecha_documento = item.FechaDocumento,
                fecha_dua = item.FechaDua,
                fecha_registro = item.FechaRegistro,
                monto_articulo_52 = item.MontoArticulo52,
                monto_flete = item.MontoFlete,
                monto_impuesto = item.MontoImpuesto,
                monto_multa = item.MontoMulta,
                monto_otros_gastos = item.MontoOtrosGastos,
                monto_seguro = item.MontoSeguro,
                no_conocimiento_embarque = item.NoConocimientoEmbarque,
                no_documento = item.NoDocumento,
                no_dua = item.NoDua,
                no_serie_id = item.NoSerieId.Value,
                nombre_proveedor = item.NombreProveedor,
                tasa_aduana = item.TasaAduana,
                tasa_dolar = item.TasaDolar,
                total_cif_general = item.TotalCifGeneral,
                total_gasto_manejo = item.TotalGastoManejo,
                total_gravamen_general = item.TotalGravamenGeneral,
                total_itbis_general = item.TotalItbisGeneral,
                total_selectivo_general = item.TotalSelectivoGeneral,
                fecha_creado = item.FechaCreado!.Value,
                creado_por = item.CreadoPor!,
                fecha_modificado = item.FechaModificado,
                modificado_por = item.ModificadoPor,
            };
            if (item.Lineas != null)
            {
                foreach (var detail in item.Lineas)
                {
                    histItem.HistLineaLiquidaciones.Add(
                        new DataBase.Models.HistLineaLiquidaciones
                        {
                            id_hist_linea_liquidacion = detail.IdLineaLiquidacion!.Value,
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
            }
            if (item.Cargos != null)
            {
                foreach (var detail in item.Cargos)
                {
                    histItem.HistCargosAdicionales.Add(
                        new DataBase.Models.HistCargosAdicionales
                        {
                            descripcion_cargo_producto = detail.DescripcionCargoProducto,
                            fecha_documento = detail.FechaDocumento,
                            hist_cabecera_liquidacion_id = detail.CabeceraLiquidacionId,
                            id_cargo_producto = detail.IdCargoProducto,
                            id_hist_cargo_adicional = detail.IdCargoAdicional!.Value,
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
            }
            _context.HistCabeceraLiquidaciones.Add(histItem);

            //SE ELIMINAN LOS DETALLES DE LA TABLA PRODUCCION
            var detailsDB = _context.LineaLiquidaciones.Where(i => i.cabecera_liquidacion_id == id);
            _context.LineaLiquidaciones.RemoveRange(detailsDB);

            //SE ELIMINAN LOS DETALLES DE LA TABLA PRODUCCION
            var detailsCargosDB = _context.CargosAdicionales.Where(i =>
                i.cabecera_liquidacion_id == id
            );
            _context.CargosAdicionales.RemoveRange(detailsCargosDB);

            //SE ELIMINA EL ENCABEZADO DE LA TABLA DE PRODUCCION
            var itemDB = _context
                .CabeceraLiquidaciones.Where(i => i.id_cabecera_liquidacion == id)
                .First();
            _context.CabeceraLiquidaciones.Remove(itemDB);
            _context.SaveChanges();

            //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
            await RefrescarCache(true);
            return new Resultado { Exito = true };
        }
        return new Resultado
        {
            Exito = false,
            Mensaje = string.Format(
                "No se encontró la Liquidación {0} para archivar",
                id.ToString()
            ),
        };
    }

    public async Task<List<LineaLiquidacion>> ObtenerLineasLiquidacion(List<int> transitos)
    {
        var lineasLiquidacion = new List<LineaLiquidacion>();

        var todosTransitosLineas = _context
            .LineaTransitos.Include(i => i.hist_cabecera_llegada)
            .ThenInclude(i => i.HistLineaLlegadas)
            .Include(i => i.cabecera_transito)
            .Where(i => transitos.Contains(i.cabecera_transito_id))
            .ToList();

        var todasLlegadasIds = todosTransitosLineas
            .Where(i => transitos.Contains(i.cabecera_transito_id))
            .Select(i => i.hist_cabecera_llegada_id)
            .Distinct()
            .ToList();

        var todasLlegadasLineas = todosTransitosLineas
            .Where(i => todasLlegadasIds.Contains(i.hist_cabecera_llegada_id))
            .Select(i => i.hist_cabecera_llegada.HistLineaLlegadas)
            .ToList()
            .SelectMany(x => x)
            .OrderBy(i => i.id_hist_linea_llegada)
            .ToList();

        var todosCodigosArancelariosIds = todasLlegadasLineas
            .Select(i => i.id_cod_arancelario)
            .Distinct()
            .ToList();

        var todosCodigosArancelarios = _context
            .CodigoArancelarios.Where(i =>
                todosCodigosArancelariosIds.Contains(i.id_cod_arancelario)
            )
            .ToList();

        foreach (var idTransito in transitos)
        {
            var transito = todosTransitosLineas
                .Where(i => i.cabecera_transito_id == idTransito)
                .First()
                .cabecera_transito;

            var llegadasIdsxTransito = todosTransitosLineas
                .Where(i => i.cabecera_transito_id == idTransito)
                .Select(i => i.hist_cabecera_llegada_id)
                .ToList();

            var llegadasLineasxTransito = todasLlegadasLineas
                .Where(i => llegadasIdsxTransito.Contains(i.hist_cabecera_llegada_id))
                .ToList();

            var codigosArancelarios = llegadasLineasxTransito
                .Select(i => i.id_cod_arancelario)
                .Distinct()
                .ToList();

            var listCodigosArancelarios = todosCodigosArancelarios
                .Where(i => codigosArancelarios.Contains(i.id_cod_arancelario))
                .ToList();

            foreach (var llegadaLinea in llegadasLineasxTransito)
            {
                var codArancelario = listCodigosArancelarios
                    .Where(j =>
                        j.id_cod_arancelario == llegadaLinea.id_cod_arancelario
                        && j.id_pais == llegadaLinea.id_pais
                    )
                    .FirstOrDefault();
                if (codArancelario != null)
                {
                    lineasLiquidacion.Add(
                        new LineaLiquidacion
                        {
                            Cantidad = llegadaLinea.cantidad,
                            IdProducto = llegadaLinea.id_producto,
                            NoProducto = llegadaLinea.no_producto,
                            DescripcionProducto = llegadaLinea.descripcion_producto,
                            ReferenciaProducto = llegadaLinea.referencia_producto,
                            HistCabeceraTransitoId = idTransito,
                            HistCabeceraLlegadaId = llegadaLinea.hist_cabecera_llegada_id,
                            IdHistLineaLlegada = llegadaLinea.id_hist_linea_llegada,
                            AlmacenCodigo = transito!.almacen_codigo,
                            AlmacenId = transito!.almacen_id,
                            CodigoPaisOrigen = llegadaLinea.codigo_pais_origen,
                            IdPaisOrigen = llegadaLinea.id_pais,
                            IdUnidadMedida = llegadaLinea.id_unidad_medida,
                            CodigoUnidadMedida = llegadaLinea.codigo_unidad_medida,
                            FobAduanaUs = llegadaLinea.costo_unitario_directo,
                            CostoProductoUs = llegadaLinea.costo_unitario_directo,
                            CodigoArancelarioId = codArancelario.id_codigo_arancelario,
                            CodigoArancelarioCod = codArancelario.no_cod_arancelario,
                            ImporteUs = llegadaLinea.costo_unitario_directo * llegadaLinea.cantidad,
                            TasaItbis = 18,
                            UltimoCostoProductoRd = llegadaLinea.costo_unitario_directo,
                        }
                    );
                }
                else
                {
                    throw new Exception(
                        string.Format(
                            "No existe el código arancelario {0} para el país {1} en el ítem {2} de la llegada {3} incluida en el tránsito {4}",
                            llegadaLinea.no_cod_arancelario,
                            llegadaLinea.codigo_pais_origen,
                            llegadaLinea.no_producto,
                            llegadaLinea.hist_cabecera_llegada.no_documento,
                            transito.no_documento
                        )
                    );
                }
            }
        }
        return lineasLiquidacion
            .OrderBy(i => i.HistCabeceraTransitoId)
            .ThenBy(i => i.HistCabeceraLlegadaId)
            .ThenBy(i => i.IdHistLineaLlegada)
            .ToList();
    }

    public async Task<Resultado> CrearTransferencias(int id)
    {
        var resultado = await _servicioOrdenTransferencia.CrearTransferencias(id);
        if (resultado.Exito)
        {
            await Archivar(id);
        }
        return resultado;
    }

    public async Task<bool> RefrescarCache(bool incluyeHistorico = false)
    {
        _memoryCache.Remove("Liquidaciones");
        await ObtenerLiquidaciones();
        if (incluyeHistorico)
        {
            await _servicioHistLiquidacion.RefrescarCache();
        }
        return true;
    }

    private bool ExisteTransitoEnOtraLiquidacion(int idTransito, int? idLiquidacion)
    {
        int items = 0;
        int itemsHistorico = 0;
        if (idLiquidacion.HasValue)
        {
            items = _context
                .LineaLiquidaciones.Where(i =>
                    i.hist_cabecera_transito_id == idTransito
                    && i.cabecera_liquidacion_id != idLiquidacion.Value
                )
                .Count();
            itemsHistorico = _context
                .HistLineaLiquidaciones.Where(i =>
                    i.hist_cabecera_transito_id == idTransito
                    && i.hist_cabecera_liquidacion_id != idLiquidacion.Value
                )
                .Count();
        }
        else
        {
            items = _context
                .LineaLiquidaciones.Where(i => i.hist_cabecera_transito_id == idTransito)
                .Count();
            itemsHistorico = _context
                .HistLineaLiquidaciones.Where(i => i.hist_cabecera_transito_id == idTransito)
                .Count();
        }
        return items == 0 && itemsHistorico == 0 ? false : true;
    }

    private bool CalcularTotalesLineas(DataBase.Models.CabeceraLiquidaciones item)
    {
        try
        {
            var lineas = _context
                .LineaLiquidaciones.Where(i =>
                    i.cabecera_liquidacion_id == item.id_cabecera_liquidacion
                )
                .ToList();

            var codigosArancelariosIds = lineas
                .Select(i => i.codigo_arancelario_id)
                .Distinct()
                .ToList();

            var codigosArancelarios = _context
                .CodigoArancelarios.Where(i =>
                    codigosArancelariosIds.Contains(i.id_codigo_arancelario)
                )
                .ToList();

            foreach (var linea in lineas)
            {
                var codigoArancelario = codigosArancelarios.First(c =>
                    c.id_codigo_arancelario == linea.codigo_arancelario_id
                    && c.id_pais == linea.id_pais_origen
                );

                var otrasLineas = lineas
                    .Where(i => i.id_linea_liquidacion != linea.id_linea_liquidacion)
                    .ToList();

                linea.total_cif = CalcularLineaTotalCif(
                    linea.fob_aduana_us,
                    linea.cantidad,
                    item.monto_seguro,
                    item.monto_flete,
                    item.monto_otros_gastos,
                    item.tasa_aduana,
                    otrasLineas.Sum(i => i.fob_aduana_us * i.cantidad)
                );

                linea.total_gravamen = CalcularLineaTotalGravamen(
                    linea.total_cif,
                    codigoArancelario.porciento_gravamen
                );

                linea.total_selectivo = CalcularLineaTotalSelectivo(
                    linea.total_cif,
                    codigoArancelario.porciento_selectivo,
                    linea.total_gravamen
                );

                linea.total_itbis = CalcularLineaTotalItbis(
                    linea.total_cif,
                    linea.total_gravamen,
                    linea.total_selectivo,
                    linea.tasa_itbis
                );

                linea.total_general = CalcularLineaTotalGeneral(
                    linea.total_gravamen,
                    linea.total_selectivo,
                    linea.total_itbis
                );

                linea.costo_producto_rd = CalcularLineaCostoRD(
                    linea.costo_producto_us,
                    linea.cantidad,
                    item.tasa_dolar,
                    linea.total_gravamen,
                    linea.total_selectivo,
                    item.total_gasto_manejo,
                    otrasLineas.Sum(i => i.costo_producto_us * i.cantidad)
                );

                linea.tasa_liquidacion = CalcularLineaTasaLiquidacion(
                    linea.costo_producto_rd,
                    linea.costo_producto_us
                );

                _context.SaveChanges();
            }
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private bool CalcularTotalesCabecera(int idLiquidacion)
    {
        var lineas = _context.LineaLiquidaciones.Where(i =>
            i.cabecera_liquidacion_id == idLiquidacion
        );
        var oldItem = _context
            .CabeceraLiquidaciones.Where(i => i.id_cabecera_liquidacion == idLiquidacion)
            .FirstOrDefault();

        oldItem.total_cif_general = lineas.Sum(i => i.total_cif);
        oldItem.total_gravamen_general = lineas.Sum(i => i.total_gravamen);
        oldItem.total_selectivo_general = lineas.Sum(i => i.total_selectivo);
        oldItem.total_itbis_general = lineas.Sum(i => i.total_itbis);
        oldItem.monto_impuesto = lineas.Sum(i => i.total_general);
        _context.SaveChanges();
        return true;
    }

    private bool CalcularTotalesCabeceraCargosAdicionales(int idLiquidacion)
    {
        var parent = _context
            .CabeceraLiquidaciones.Where(i => i.id_cabecera_liquidacion == idLiquidacion)
            .First();
        parent.total_gasto_manejo = _context
            .CargosAdicionales.Where(i => i.cabecera_liquidacion_id == idLiquidacion)
            .Sum(i => i.monto_documento);
        _context.SaveChanges();

        CalcularTotalesLineas(parent);

        CalcularTotalesCabecera(parent.id_cabecera_liquidacion);

        return true;
    }

    private decimal CalcularLineaTotalCif(
        decimal fob_aduana_us,
        decimal cantidad,
        decimal montoSeguro,
        decimal montoFlete,
        decimal montoOtrosGastos,
        decimal tasaAduana,
        decimal totalGeneralFobAduana
    )
    {
        totalGeneralFobAduana += (cantidad * fob_aduana_us);

        return (fob_aduana_us * cantidad)
            * (1 + ((montoSeguro + montoFlete + montoOtrosGastos) / totalGeneralFobAduana))
            * tasaAduana;
    }

    private decimal CalcularLineaTotalGravamen(decimal totalCif, decimal porcientoGravamen)
    {
        return (totalCif * porcientoGravamen) / 100;
    }

    private decimal CalcularLineaTotalSelectivo(
        decimal totalCif,
        decimal porcientoSelectivo,
        decimal totalGravamen
    )
    {
        return ((totalCif + totalGravamen) * porcientoSelectivo) / 100;
    }

    private decimal CalcularLineaTotalItbis(
        decimal totalCif,
        decimal totalGravamen,
        decimal totalSelectivo,
        decimal tasaItbis
    )
    {
        return ((totalCif + totalGravamen + totalSelectivo) * tasaItbis) / 100;
    }

    private decimal CalcularLineaTotalGeneral(
        decimal totalGravamen,
        decimal totalSelectivo,
        decimal totalItbis
    )
    {
        return totalGravamen + totalSelectivo + totalItbis;
    }

    private decimal CalcularLineaCostoRD(
        decimal costo,
        decimal cantidad,
        decimal tasaDolar,
        decimal totalGravamen,
        decimal totalSelectivo,
        decimal totalGastoManejo,
        decimal totalGeneralMontoUs
    )
    {
        totalGeneralMontoUs += (cantidad * costo);
        decimal montoCostoUs = costo * cantidad;
        decimal precioRd = montoCostoUs * tasaDolar;
        decimal impuesto = totalGravamen + totalSelectivo;
        decimal promedioGasto = (totalGastoManejo / totalGeneralMontoUs) * montoCostoUs;
        decimal costoInvertido = precioRd + impuesto + promedioGasto;
        return costoInvertido / cantidad;
    }

    private decimal CalcularLineaTasaLiquidacion(decimal costoRd, decimal costoUs)
    {
        return costoRd / costoUs;
    }
}
