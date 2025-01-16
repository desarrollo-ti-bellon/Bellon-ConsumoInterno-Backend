using System.Linq.Expressions;
using System.Runtime.CompilerServices;
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

public class ServicioLlegada : IServicioLlegada
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;
    private readonly IServicioNumeroSerie _servicioNumeroSerie;
    private readonly IServicioHistLlegada _servicioHistLlegada;

    public ServicioLlegada(
        IHttpContextAccessor httpContextAccessor,
        DataBase.AppDataBase context,
        IOptions<AppSettings> settings,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IServicioAutorizacion servicioAutorizacion,
        IServicioHistLlegada servicioHistLlegada,
        IServicioNumeroSerie servicioNumeroSerie
    )
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _servicioAutorizacion = servicioAutorizacion;
        _servicioHistLlegada = servicioHistLlegada;
        _servicioNumeroSerie = servicioNumeroSerie;
    }

    public async Task<List<CabeceraLlegada>> ObtenerLlegadas()
    {
        var cache = _memoryCache.Get<List<CabeceraLlegada>>("Llegadas");
        if (cache == null)
        {
            cache = _context
                .CabeceraLlegadas.Select(i => new CabeceraLlegada
                {
                    IdCabeceraLlegada = i.id_cabecera_llegada,
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
                    CantidadLineas = i.LineaLlegadas.Count,
                })
                .ToList();
            _memoryCache.Set<List<CabeceraLlegada>>(
                "Llegadas",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return cache.OrderBy(i => i.IdCabeceraLlegada.Value).ToList();
        ;
    }

    public async Task<CabeceraLlegada> ObtenerLlegada(int id)
    {
        var allItems = await ObtenerLlegadas();
        var item = allItems.Where(i => i.IdCabeceraLlegada == id).FirstOrDefault().Clone();
        if (item != null)
        {
            item.Lineas = _context
                .LineaLlegadas.Where(i => i.cabecera_llegada_id == id)
                .Select(i => new LineaLlegada
                {
                    AlmacenCodigo = i.almacen_codigo,
                    AlmacenId = i.almacen_id,
                    CabeceraLlegadaId = i.cabecera_llegada_id,
                    Cantidad = i.cantidad,
                    CantidadOrigen = i.cantidad_origen,
                    CodigoPaisOrigen = i.codigo_pais_origen,
                    CodigoUnidadMedida = i.codigo_unidad_medida,
                    CostoUnitario = i.costo_unitario,
                    CostoUnitarioDirecto = i.costo_unitario_directo,
                    CreadoPor = i.creado_por,
                    DescripcionProducto = i.descripcion_producto,
                    FechaCreado = i.fecha_creado,
                    IdLineaLlegada = i.id_linea_llegada,
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
                .OrderBy(i => i.IdLineaLlegada)
                .ToList();
        }
        return item;
    }

    public async Task<CabeceraLlegada> GuardarCabeceraLlegada(CabeceraLlegada item)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (item.IdCabeceraLlegada.HasValue)
        {
            var oldItem = _context
                .CabeceraLlegadas.Where(i => i.id_cabecera_llegada == item.IdCabeceraLlegada.Value)
                .FirstOrDefault();
            if (oldItem != null)
            {
                //ACTUALIZA EL OBJETO EXISTENTE
                oldItem.agente_id = item.AgenteId;
                oldItem.cantidad_peso = item.CantidadPeso;
                oldItem.cantidad_pieza = item.CantidadPieza;
                oldItem.cantidad_volumen = item.CantidadVolumen;
                oldItem.codigo_peso = item.CodigoPeso;
                oldItem.codigo_volumen = item.CodigoVolumen;
                oldItem.fecha_documento = item.FechaDocumento;
                oldItem.fecha_registro = item.FechaRegistro;
                oldItem.id_documento_origen = item.IdDocumentoOrigen;
                oldItem.id_peso = item.IdPeso;
                oldItem.id_proveedor = item.IdProveedor;
                oldItem.id_volumen = item.IdVolumen;
                oldItem.no_almacen_us = item.NoAlmacenUS;
                oldItem.no_documento_origen = item.NoDocumentoOrigen;
                oldItem.no_documento_previo = item.NoDocumentoPrevio;
                oldItem.no_factura_proveedor = item.NoFacturaProveedor;
                oldItem.no_proveedor = item.NoProveedor;
                oldItem.nombre_proveedor = item.NombreProveedor;
                oldItem.transportista = item.Transportista;
                oldItem.total = item.Total;
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
                return await ObtenerLlegada(oldItem.id_cabecera_llegada);
            }
        }
        else
        {
            //SE INSERTA EL NUEVO ITEM
            var numeroSerie = await _servicioNumeroSerie.ObtenerNumeroDocumento(
                _settings.DocumentoLLegadaNoSerieId
            );
            var newItemData = new DataBase.Models.CabeceraLlegadas
            {
                agente_id = item.AgenteId,
                cantidad_peso = item.CantidadPeso,
                cantidad_pieza = item.CantidadPieza,
                cantidad_volumen = item.CantidadVolumen,
                codigo_peso = item.CodigoPeso,
                codigo_volumen = item.CodigoVolumen,
                fecha_documento = item.FechaDocumento,
                fecha_registro = item.FechaRegistro,
                id_documento_origen = item.IdDocumentoOrigen,
                id_peso = item.IdPeso,
                id_proveedor = item.IdProveedor,
                id_volumen = item.IdVolumen,
                no_almacen_us = item.NoAlmacenUS,
                no_documento = numeroSerie,
                no_serie_id = _settings.DocumentoLLegadaNoSerieId,
                no_documento_origen = item.NoDocumentoOrigen,
                no_documento_previo = item.NoDocumentoPrevio,
                no_factura_proveedor = item.NoFacturaProveedor,
                no_proveedor = item.NoProveedor,
                nombre_proveedor = item.NombreProveedor,
                total = 0,
                transportista = item.Transportista,
                fecha_creado = DateTime.UtcNow,
                creado_por = identity!.Name!,
            };
            var newItem = _context.CabeceraLlegadas.Add(newItemData);
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
            return await ObtenerLlegada(newItem.Entity.id_cabecera_llegada);
        }
        return null;
    }

    public async Task<CabeceraLlegada> GuardarLineaLlegada(List<LineaLlegada> items)
    {
        if (items.Count > 0)
        {
            var parent = _context
                .CabeceraLlegadas.Where(i => i.id_cabecera_llegada == items[0].CabeceraLlegadaId)
                .FirstOrDefault();
            if (parent != null)
            {
                var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                foreach (var item in items)
                {
                    if (item.IdLineaLlegada.HasValue)
                    {
                        var oldItem = _context
                            .LineaLlegadas.Where(i =>
                                i.id_linea_llegada == item.IdLineaLlegada.Value
                                && i.cabecera_llegada_id == item.CabeceraLlegadaId
                            )
                            .FirstOrDefault();
                        if (oldItem != null)
                        {
                            var validarCantidadItemExistente = ValidarCantidad(
                                item,
                                parent,
                                oldItem.cantidad
                            );
                            //ACTUALIZA EL OBJETO EXISTENTE
                            if (validarCantidadItemExistente.Exito)
                            {
                                oldItem.cabecera_llegada_id = item.CabeceraLlegadaId;
                                oldItem.almacen_codigo = item.AlmacenCodigo;
                                oldItem.almacen_id = item.AlmacenId;
                                oldItem.cantidad = item.Cantidad;
                                oldItem.cantidad_origen = item.CantidadOrigen;
                                oldItem.codigo_pais_origen = item.CodigoPaisOrigen;
                                oldItem.codigo_unidad_medida = item.CodigoUnidadMedida;
                                oldItem.costo_unitario = item.CostoUnitario;
                                oldItem.costo_unitario_directo = item.CostoUnitarioDirecto;
                                oldItem.descripcion_producto = item.DescripcionProducto;
                                oldItem.id_pais = item.IdPais;
                                oldItem.id_producto = item.IdProducto;
                                oldItem.id_unidad_medida = item.IdUnidadMedida;
                                oldItem.no_producto = item.NoProducto;
                                oldItem.precio_unitario = item.PrecioUnitario;
                                oldItem.referencia_producto = item.ReferenciaProducto;
                                oldItem.total = item.Total;
                                oldItem.id_cod_arancelario = item.IdCodArancelario;
                                oldItem.no_cod_arancelario = item.NoCodArancelario;
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
                            else
                            {
                                throw new Exception(
                                    "Error al actualizar el registro: <"
                                        + validarCantidadItemExistente.Mensaje
                                        + ">"
                                );
                            }
                        }
                    }
                    else
                    {
                        var validarCantidadItemNuevo = ValidarCantidad(item, parent);
                        if (validarCantidadItemNuevo.Exito)
                        {
                            //SE INSERTA EL NUEVO ITEM
                            var newItemData = new DataBase.Models.LineaLlegadas
                            {
                                cabecera_llegada_id = item.CabeceraLlegadaId,
                                almacen_codigo = item.AlmacenCodigo,
                                almacen_id = item.AlmacenId,
                                cantidad = item.Cantidad,
                                cantidad_origen = item.CantidadOrigen,
                                codigo_pais_origen = item.CodigoPaisOrigen,
                                codigo_unidad_medida = item.CodigoUnidadMedida,
                                costo_unitario = item.CostoUnitario,
                                costo_unitario_directo = item.CostoUnitarioDirecto,
                                descripcion_producto = item.DescripcionProducto,
                                id_pais = item.IdPais,
                                id_producto = item.IdProducto,
                                id_unidad_medida = item.IdUnidadMedida,
                                no_producto = item.NoProducto,
                                precio_unitario = item.PrecioUnitario,
                                referencia_producto = item.ReferenciaProducto,
                                total = item.Total,
                                id_cod_arancelario = item.IdCodArancelario,
                                no_cod_arancelario = item.NoCodArancelario,
                                fecha_creado = DateTime.UtcNow,
                                creado_por = identity!.Name!,
                            };
                            var newItem = _context.LineaLlegadas.Add(newItemData);
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
                                "Error al crear el registro: <"
                                    + validarCantidadItemNuevo.Mensaje
                                    + ">"
                            );
                        }
                    }
                }
                //SE ACTUALIZA EL TOTAL DE LA CABECERA
                CalcularTotalCabecera(items[0].CabeceraLlegadaId);

                //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
                await RefrescarCache();

                //SE RETORNA EL OBJETO CREADO
                return await ObtenerLlegada(items[0].CabeceraLlegadaId);
            }
            else
            {
                throw new Exception(
                    string.Format("No se encontró la Llegada {0}", items[0].CabeceraLlegadaId)
                );
            }
        }
        else
        {
            throw new Exception("No hay items para insertar");
        }
    }

    public async Task<CabeceraLlegada> EliminarLineaLlegada(int item)
    {
        var oldItem = _context
            .LineaLlegadas.Where(i => i.id_linea_llegada == item)
            .FirstOrDefault();
        if (oldItem != null)
        {
            _context.LineaLlegadas.Remove(oldItem);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el registro: <" + ex.Message + ">");
            }

            //SE ACTUALIZA EL TOTAL DE LA CABECERA
            CalcularTotalCabecera(oldItem.cabecera_llegada_id);

            //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
            await RefrescarCache();

            return await ObtenerLlegada(oldItem.cabecera_llegada_id);
        }
        return null;
    }

    public void CalcularTotalCabecera(int id)
    {
        var item = _context
            .CabeceraLlegadas.Where(i => i.id_cabecera_llegada == id)
            .FirstOrDefault();
        if (item != null)
        {
            item.total = _context
                .LineaLlegadas.Where(i => i.cabecera_llegada_id == id)
                .Sum(i => i.total);
            _context.SaveChanges();
        }
    }

    public async Task<int> ObtenerActivas()
    {
        return (await ObtenerLlegadas()).Count;
    }

    public async Task<Resultado> Archivar(int id)
    {
        var item = await ObtenerLlegada(id);
        if (item != null)
        {
            //SE CREA EL ITEM EN LA TABLA DE HISTORICO
            var histItem = new DataBase.Models.HistCabeceraLlegadas
            {
                id_hist_cabecera_llegada = item.IdCabeceraLlegada!.Value,
                agente_id = item.AgenteId,
                cantidad_peso = item.CantidadPeso,
                cantidad_pieza = item.CantidadPieza,
                cantidad_volumen = item.CantidadVolumen,
                codigo_peso = item.CodigoPeso,
                codigo_volumen = item.CodigoVolumen,
                fecha_documento = item.FechaDocumento,
                fecha_registro = item.FechaRegistro,
                id_documento_origen = item.IdDocumentoOrigen,
                id_peso = item.IdPeso,
                id_proveedor = item.IdProveedor,
                id_volumen = item.IdVolumen,
                no_almacen_us = item.NoAlmacenUS,
                no_documento = item.NoDocumento,
                no_documento_origen = item.NoDocumentoOrigen,
                no_documento_previo = item.NoDocumentoPrevio,
                no_factura_proveedor = item.NoFacturaProveedor,
                no_proveedor = item.NoProveedor,
                no_serie_id = item.NoSerieId.Value,
                nombre_proveedor = item.NombreProveedor,
                total = item.Total,
                transportista = item.Transportista,
                fecha_creado = item.FechaCreado!.Value,
                creado_por = item.CreadoPor!,
                fecha_modificado = item.FechaModificado,
                modificado_por = item.ModificadoPor,
            };
            if (item.Lineas != null)
            {
                foreach (var detail in item.Lineas)
                {
                    histItem.HistLineaLlegadas.Add(
                        new DataBase.Models.HistLineaLlegadas
                        {
                            id_hist_linea_llegada = detail.IdLineaLlegada!.Value,
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
                            fecha_modificado = detail.FechaModificado,
                            id_pais = detail.IdPais,
                            id_producto = detail.IdProducto,
                            id_unidad_medida = detail.IdUnidadMedida,
                            modificado_por = detail.ModificadoPor,
                            no_producto = detail.NoProducto,
                            precio_unitario = detail.PrecioUnitario,
                            referencia_producto = detail.ReferenciaProducto,
                            total = detail.Total,
                            id_cod_arancelario = detail.IdCodArancelario,
                            no_cod_arancelario = detail.NoCodArancelario,
                        }
                    );
                }
            }
            _context.HistCabeceraLlegadas.Add(histItem);

            //SE ELIMINAN LOS DETALLES DE LA TABLA PRODUCCION
            var detailsDB = _context.LineaLlegadas.Where(i => i.cabecera_llegada_id == id);
            _context.LineaLlegadas.RemoveRange(detailsDB);

            //SE ELIMINA EL ENCABEZADO DE LA TABLA DE PRODUCCION
            var itemDB = _context.CabeceraLlegadas.Where(i => i.id_cabecera_llegada == id).First();
            _context.CabeceraLlegadas.Remove(itemDB);
            _context.SaveChanges();

            //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
            await RefrescarCache(true);
            return new Resultado { Exito = true };
        }
        return new Resultado
        {
            Exito = false,
            Mensaje = string.Format("No se encontró la llegada {0} para archivar", id.ToString()),
        };
    }

    public async Task<bool> RefrescarCache(bool incluyeHistorico = false)
    {
        _memoryCache.Remove("Llegadas");
        await ObtenerLlegadas();
        if (incluyeHistorico)
        {
            await _servicioHistLlegada.RefrescarCache();
        }
        return true;
    }

    private Resultado ValidarCantidad(
        LineaLlegada item,
        DataBase.Models.CabeceraLlegadas parent,
        decimal? ValorActual = 0
    )
    {
        var itemCantidadPendiente = _context
            .RecepcionMercancia.Where(i =>
                i.documento_recepcion_mercancia == parent.no_documento_previo
                && i.id_producto == item.IdProducto
            )
            .FirstOrDefault();

        //NO HAY REGISTRO DE NINGUNA LLEGADA
        if (itemCantidadPendiente == null)
        {
            if (item.CantidadOrigen >= item.Cantidad)
            {
                return new Resultado { Exito = true };
            }
            else
            {
                return new Resultado
                {
                    Exito = false,
                    Mensaje = string.Format(
                        "La cantidad no puede ser mayor a {0:F2}",
                        item.CantidadOrigen
                    ),
                };
            }
        }
        else
        {
            var cantidadPendiente =
                itemCantidadPendiente.cantidad_origen
                - itemCantidadPendiente.cantidad_recibida
                + ValorActual;
            if (cantidadPendiente >= item.Cantidad)
            {
                return new Resultado { Exito = true };
            }
            else
            {
                return new Resultado
                {
                    Exito = false,
                    Mensaje = string.Format(
                        "La cantidad no puede ser mayor a {0:F2}",
                        cantidadPendiente
                    ),
                };
            }
        }
    }
}
