using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using Bellon.API.Liquidacion.Classes;
using Bellon.API.Liquidacion.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Bellon.API.Liquidacion.Services;

public class ServicioTransito : IServicioTransito
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;
    private readonly IServicioLlegada _servicioLlegada;
    private readonly IServicioHistLlegada _servicioHistLlegada;
    private readonly IServicioHistTransito _servicioHistTransito;
    private readonly IServicioNumeroSerie _servicioNumeroSerie;

    public ServicioTransito(
        IHttpContextAccessor httpContextAccessor,
        DataBase.AppDataBase context,
        IOptions<AppSettings> settings,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IServicioAutorizacion servicioAutorizacion,
        IServicioLlegada servicioLlegada,
        IServicioHistLlegada servicioHistLlegada,
        IServicioHistTransito servicioHistTransito,
        IServicioNumeroSerie servicioNumeroSerie
    )
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _servicioAutorizacion = servicioAutorizacion;
        _servicioLlegada = servicioLlegada;
        _servicioHistLlegada = servicioHistLlegada;
        _servicioHistTransito = servicioHistTransito;
        _servicioNumeroSerie = servicioNumeroSerie;
    }

    public async Task<List<CabeceraTransito>> ObtenerTransitos()
    {
        var cache = _memoryCache.Get<List<CabeceraTransito>>("Transitos");
        if (cache == null)
        {
            cache = _context
                .CabeceraTransito.Select(i => new CabeceraTransito
                {
                    IdCabeceraTransito = i.id_cabecera_transito,
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
                    CantidadLineas = i.LineaTransitos.Count,
                })
                .ToList();
            _memoryCache.Set<List<CabeceraTransito>>(
                "Transitos",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return cache.OrderBy(i => i.IdCabeceraTransito.Value).ToList();
    }

    public async Task<CabeceraTransito> ObtenerTransito(int id)
    {
        var allItems = await ObtenerTransitos();
        var item = allItems.Where(i => i.IdCabeceraTransito == id).FirstOrDefault().Clone();
        if (item != null)
        {
            item.Lineas = _context
                .LineaTransitos.Include(i => i.hist_cabecera_llegada)
                .Where(i => i.cabecera_transito_id == id)
                .Select(i => new LineaTransito
                {
                    IdLineaTransito = i.id_linea_transito,
                    CabeceraTransitoId = i.cabecera_transito_id,
                    HistCabeceraLlegadaId = i.hist_cabecera_llegada_id,
                    HistCabeceraLlegada = new CabeceraLlegada
                    {
                        IdCabeceraLlegada = i.hist_cabecera_llegada.id_hist_cabecera_llegada,
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
                    FechaModificado = i.fecha_modificado,
                    ModificadoPor = i.modificado_por,
                    NoLlegada = i.no_llegada,
                    CreadoPor = i.creado_por,
                    FechaCreado = i.fecha_creado,
                })
                .OrderBy(i => i.IdLineaTransito)
                .ToList();
        }
        return item;
    }

    public async Task<CabeceraTransito> GuardarCabeceraTransito(CabeceraTransito item)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (item.IdCabeceraTransito.HasValue)
        {
            var oldItem = _context
                .CabeceraTransito.Where(i =>
                    i.id_cabecera_transito == item.IdCabeceraTransito.Value
                )
                .FirstOrDefault();
            if (oldItem != null)
            {
                //ACTUALIZA EL OBJETO EXISTENTE
                oldItem.almacen_codigo = item.AlmacenCodigo;
                oldItem.almacen_id = item.AlmacenId;
                oldItem.detalle_mercancia = item.DetalleMercancia;
                oldItem.fecha_desembarque = item.FechaDesembarque;
                oldItem.fecha_documento = item.FechaDocumento;
                oldItem.fecha_embarque = item.FechaEmbarque;
                oldItem.fecha_estimada = item.FechaEstimada;
                oldItem.naviera = item.Naviera;
                oldItem.no_buque = item.NoBuque;
                oldItem.no_conocimiento_embarque = item.NoConocimientoEmbarque;
                oldItem.no_contenedor = item.NoContenedor;
                oldItem.no_sello = item.NoSello;
                oldItem.nombre_proveedor = item.NombreProveedor;
                oldItem.puerto_desembarque = item.PuertoDesembarque;
                oldItem.puerto_embarque = item.PuertoEmbarque;
                oldItem.tipo_contenedor_id = item.TipoContenedor_id;
                oldItem.fecha_modificado = DateTime.UtcNow;
                oldItem.modificado_por = identity!.Name;
                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al actualizar el registro: <" + ex.Message + ">");
                }

                //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
                await RefrescarCache();

                //SE RETORNA EL OBJETO MODIFICADO
                return await ObtenerTransito(oldItem.id_cabecera_transito);
            }
        }
        else
        {
            //SE INSERTA EL NUEVO ITEM
            var numeroSerie = await _servicioNumeroSerie.ObtenerNumeroDocumento(
                _settings.DocumentoTransitoNoSerieId
            );
            var newItemData = new DataBase.Models.CabeceraTransito
            {
                almacen_codigo = item.AlmacenCodigo,
                almacen_id = item.AlmacenId,
                detalle_mercancia = item.DetalleMercancia,
                fecha_desembarque = item.FechaDesembarque,
                fecha_documento = item.FechaDocumento,
                fecha_embarque = item.FechaEmbarque,
                fecha_estimada = item.FechaEstimada,
                fecha_registro = item.FechaRegistro,
                naviera = item.Naviera,
                no_buque = item.NoBuque,
                no_conocimiento_embarque = item.NoConocimientoEmbarque,
                no_contenedor = item.NoContenedor,
                no_documento = numeroSerie,
                no_serie_id = _settings.DocumentoTransitoNoSerieId,
                no_sello = item.NoSello,
                nombre_proveedor = item.NombreProveedor,
                puerto_desembarque = item.PuertoDesembarque,
                puerto_embarque = item.PuertoEmbarque,
                tipo_contenedor_id = item.TipoContenedor_id,
                total = 0,
                fecha_creado = DateTime.UtcNow,
                creado_por = identity!.Name!,
            };
            var newItem = _context.CabeceraTransito.Add(newItemData);
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
            return await ObtenerTransito(newItem.Entity.id_cabecera_transito);
        }
        return null;
    }

    public async Task<CabeceraTransito> GuardarLineaTransito(List<LineaTransito> items)
    {
        if (items.Count > 0)
        {
            var parent = _context
                .CabeceraTransito.Where(i => i.id_cabecera_transito == items[0].CabeceraTransitoId)
                .FirstOrDefault();
            if (parent != null)
            {
                var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                foreach (var item in items)
                {
                    if (item.HistCabeceraLlegadaId > 0)
                    {
                        if (
                            ExisteLlegadaEnOtroTransito(
                                item.HistCabeceraLlegadaId,
                                item.CabeceraTransitoId
                            )
                        )
                        {
                            throw new Exception(
                                string.Format(
                                    "La llegada {0} se encuentra en otro Tránsito, por favor verifique",
                                    item.HistCabeceraLlegadaId
                                )
                            );
                        }

                        if (item.IdLineaTransito.HasValue)
                        {
                            var oldItem = _context
                                .LineaTransitos.Where(i =>
                                    i.id_linea_transito == item.IdLineaTransito.Value
                                    && i.cabecera_transito_id == item.CabeceraTransitoId
                                )
                                .FirstOrDefault();
                            if (oldItem != null)
                            {
                                //SI LA LINEA ESTA CAMBIANDO DE LLEGADA (HistCabeceraLlegadaId)
                                if (oldItem.hist_cabecera_llegada_id != item.HistCabeceraLlegadaId)
                                {
                                    //VALIDA QUE LA NUEVA LLEGADA (HistCabeceraLlegadaId) EXISTA EN PRODUCCION (ACTIVA)
                                    var llegada = _context
                                        .CabeceraLlegadas.Where(i =>
                                            i.id_cabecera_llegada == item.HistCabeceraLlegadaId
                                        )
                                        .FirstOrDefault();
                                    if (llegada != null)
                                    {
                                        var oldHistCabeceraLlegadaId =
                                            oldItem.hist_cabecera_llegada_id;

                                        //ARCHIVA (DESACTIVA) LA LLEGADA NUEVA
                                        var resultArchivar = await _servicioLlegada.Archivar(
                                            item.HistCabeceraLlegadaId
                                        );
                                        if (!resultArchivar.Exito)
                                        {
                                            throw new Exception(resultArchivar.Mensaje);
                                        }

                                        //ACTUALIZA LA LINEA
                                        oldItem.hist_cabecera_llegada_id =
                                            item.HistCabeceraLlegadaId;
                                        oldItem.no_llegada = item.NoLlegada;
                                        oldItem.modificado_por = identity!.Name!;
                                        oldItem.fecha_modificado = DateTime.UtcNow;
                                        try
                                        {
                                            _context.SaveChanges();
                                        }
                                        catch (Exception ex)
                                        {
                                            throw new Exception(
                                                "Error al actualizar el registro: <"
                                                    + ex.Message
                                                    + ">"
                                            );
                                        }

                                        //REGRESA A PRODUCCION (ACTIVA) LA LLEGADA QUE HABIA
                                        var resultActivar = await _servicioHistLlegada.Recuperar(
                                            oldHistCabeceraLlegadaId
                                        );
                                        if (!resultActivar.Exito)
                                        {
                                            throw new Exception(resultActivar.Mensaje);
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception(
                                            string.Format(
                                                "No se encontró la Llegada {0}",
                                                item.HistCabeceraLlegadaId
                                            )
                                        );
                                    }
                                }
                                //SI LA LINEA NO CAMBIA DE LLEGADA
                                else
                                {
                                    oldItem.no_llegada = item.NoLlegada;
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
                            }
                            else
                            {
                                throw new Exception(
                                    string.Format(
                                        "No se encontró la línea {0} del Tránsito {1}",
                                        item.IdLineaTransito.Value,
                                        item.CabeceraTransitoId
                                    )
                                );
                            }
                        }
                        else
                        {
                            //VALIDA QUE LA NUEVA LLEGADA (HistCabeceraLlegadaId) EXISTA EN PRODUCCION
                            var llegada = _context
                                .CabeceraLlegadas.Where(i =>
                                    i.id_cabecera_llegada == item.HistCabeceraLlegadaId
                                )
                                .FirstOrDefault();
                            if (llegada != null)
                            {
                                //ARCHIVA (DESACTIVA) LA LLEGADA NUEVA
                                var resultArchivar = await _servicioLlegada.Archivar(
                                    item.HistCabeceraLlegadaId
                                );
                                if (!resultArchivar.Exito)
                                {
                                    throw new Exception(resultArchivar.Mensaje);
                                }

                                //SE INSERTA EL NUEVO ITEM
                                var newItemData = new DataBase.Models.LineaTransitos
                                {
                                    cabecera_transito_id = item.CabeceraTransitoId,
                                    hist_cabecera_llegada_id = item.HistCabeceraLlegadaId,
                                    no_llegada = item.NoLlegada,
                                    fecha_creado = DateTime.UtcNow,
                                    creado_por = identity!.Name!,
                                };
                                var newItem = _context.LineaTransitos.Add(newItemData);
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
                                        "No se encontró la Llegada {0}",
                                        item.HistCabeceraLlegadaId
                                    )
                                );
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("El campo hist_cabecera_llegada_id es obligatorio");
                    }
                }

                //SE ACTUALIZA EL TOTAL DE LA CABECERA
                CalcularTotalCabecera(items[0].CabeceraTransitoId);

                //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
                await RefrescarCache();

                //SE RETORNA EL OBJETO MODIFICADO
                return await ObtenerTransito(items[0].CabeceraTransitoId);
            }
            else
            {
                throw new Exception(
                    string.Format("No se encontró el Tránsito {0}", items[0].CabeceraTransitoId)
                );
            }
        }
        else
        {
            throw new Exception("No hay items para insertar");
        }
    }

    public async Task<CabeceraTransito> EliminarLineaTransito(int item)
    {
        var oldItem = _context
            .LineaTransitos.Where(i => i.id_linea_transito == item)
            .FirstOrDefault();
        if (oldItem != null)
        {
            _context.LineaTransitos.Remove(oldItem);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el registro: <" + ex.Message + ">");
            }

            //REGRESA A PRODUCCION (ACTIVA) LA LLEGADA QUE HABIA
            var resultActivar = await _servicioHistLlegada.Recuperar(
                oldItem.hist_cabecera_llegada_id
            );
            if (!resultActivar.Exito)
            {
                throw new Exception(resultActivar.Mensaje);
            }

            //SE ACTUALIZA EL TOTAL DE LA CABECERA
            CalcularTotalCabecera(oldItem.cabecera_transito_id);

            //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
            await RefrescarCache();

            return await ObtenerTransito(oldItem.cabecera_transito_id);
        }
        return null;
    }

    public void CalcularTotalCabecera(int id)
    {
        var item = _context
            .CabeceraTransito.Where(i => i.id_cabecera_transito == id)
            .FirstOrDefault();
        if (item != null)
        {
            item.total = _context
                .LineaTransitos.Include(i => i.hist_cabecera_llegada)
                .Where(i => i.cabecera_transito_id == id)
                .Sum(i => i.hist_cabecera_llegada.total!.Value);
            _context.SaveChanges();
        }
    }

    public async Task<int> ObtenerActivas()
    {
        return (await ObtenerTransitos()).Count;
    }

    public async Task<Resultado> Archivar(int id)
    {
        var item = await ObtenerTransito(id);
        if (item != null)
        {
            //SE CREA EL ITEM EN LA TABLA DE HISTORICO
            var histItem = new DataBase.Models.HistCabeceraTransitos
            {
                id_hist_cabecera_transito = item.IdCabeceraTransito!.Value,
                almacen_codigo = item.AlmacenCodigo,
                almacen_id = item.AlmacenId,
                detalle_mercancia = item.DetalleMercancia,
                fecha_desembarque = item.FechaDesembarque,
                fecha_documento = item.FechaDocumento,
                fecha_embarque = item.FechaEmbarque,
                fecha_estimada = item.FechaEstimada,
                fecha_registro = item.FechaRegistro,
                naviera = item.Naviera,
                no_buque = item.NoBuque,
                no_conocimiento_embarque = item.NoConocimientoEmbarque,
                no_contenedor = item.NoContenedor,
                no_documento = item.NoDocumento,
                no_sello = item.NoSello,
                no_serie_id = item.NoSerieId.Value,
                nombre_proveedor = item.NombreProveedor,
                puerto_desembarque = item.PuertoDesembarque,
                puerto_embarque = item.PuertoEmbarque,
                tipo_contenedor_id = item.TipoContenedor_id,
                total = item.Total,
                fecha_creado = item.FechaCreado!.Value,
                creado_por = item.CreadoPor!,
                fecha_modificado = item.FechaModificado,
                modificado_por = item.ModificadoPor,
            };
            if (item.Lineas != null)
            {
                foreach (var detail in item.Lineas)
                {
                    histItem.HistLineaTransitos.Add(
                        new DataBase.Models.HistLineaTransitos
                        {
                            id_hist_linea_transito = detail.IdLineaTransito!.Value,
                            creado_por = detail.CreadoPor!,
                            fecha_creado = detail.FechaCreado!.Value,
                            fecha_modificado = detail.FechaModificado,
                            hist_cabecera_llegada_id = detail.HistCabeceraLlegadaId,
                            modificado_por = detail.ModificadoPor,
                            no_llegada = detail.NoLlegada,
                        }
                    );
                }
            }
            _context.HistCabeceraTransitos.Add(histItem);

            //SE ELIMINAN LOS DETALLES DE LA TABLA PRODUCCION
            var detailsDB = _context.LineaTransitos.Where(i => i.cabecera_transito_id == id);
            _context.LineaTransitos.RemoveRange(detailsDB);

            //SE ELIMINA EL ENCABEZADO DE LA TABLA DE PRODUCCION
            var itemDB = _context.CabeceraTransito.Where(i => i.id_cabecera_transito == id).First();
            _context.CabeceraTransito.Remove(itemDB);
            _context.SaveChanges();

            //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
            await RefrescarCache(true);
            return new Resultado { Exito = true };
        }
        return new Resultado
        {
            Exito = false,
            Mensaje = string.Format("No se encontró el Tránsito {0} para archivar", id.ToString()),
        };
    }

    public async Task<bool> RefrescarCache(bool incluyeHistorico = false)
    {
        _memoryCache.Remove("Transitos");
        await ObtenerTransitos();
        if (incluyeHistorico)
        {
            await _servicioHistTransito.RefrescarCache();
        }
        return true;
    }

    private bool ExisteLlegadaEnOtroTransito(int idLlegada, int? idTransito)
    {
        int items = 0;
        int itemsHistorico = 0;
        if (idTransito.HasValue)
        {
            items = _context
                .LineaTransitos.Where(i =>
                    i.hist_cabecera_llegada_id == idLlegada
                    && i.cabecera_transito_id != idTransito.Value
                )
                .Count();
            itemsHistorico = _context
                .HistLineaTransitos.Where(i =>
                    i.hist_cabecera_llegada_id == idLlegada
                    && i.hist_cabecera_transito_id != idTransito.Value
                )
                .Count();
        }
        else
        {
            items = _context
                .LineaTransitos.Where(i => i.hist_cabecera_llegada_id == idLlegada)
                .Count();
            itemsHistorico = _context
                .HistLineaTransitos.Where(i => i.hist_cabecera_llegada_id == idLlegada)
                .Count();
        }
        return items == 0 && itemsHistorico == 0 ? false : true;
    }
}
