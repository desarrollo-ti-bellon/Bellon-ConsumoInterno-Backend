using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Security.Claims;
using Bellon.API.ConsumoInterno.Interfaces;
using Bellon.API.ConsumoInterno.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Bellon.API.ConsumoInterno.DataBase;
using Microsoft.Data.SqlClient;

namespace Bellon.API.ConsumoInterno.Services;

public class ServicioSolicitud : IServicioSolicitud
{
    private readonly AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;
    private readonly IServicioNumeroSerie _servicioNumeroSerie;

    public ServicioSolicitud(
        IHttpContextAccessor httpContextAccessor,
        AppDataBase context,
        IOptions<AppSettings> settings,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IServicioAutorizacion servicioAutorizacion,
        IServicioNumeroSerie servicioNumeroSerie
    )
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _servicioAutorizacion = servicioAutorizacion;
        _servicioNumeroSerie = servicioNumeroSerie;
    }

    public async Task<List<CabeceraSolicitudCI>> ObtenerSolicitudesDelUsuarioSolicitantePorEstado(int? estadoSolicitudId)
    {
        var allItems = await ObtenerSolicitudes();
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        var usuario = await _context.UsuariosCI.FirstOrDefaultAsync(el => el.correo == identity.Name);

        if (usuario != null)
        {
            switch (usuario.posicion_id)
            {
                case 1:  // Administrador
                    return allItems.Where(i => i.IdSucursal == usuario.id_sucursal && i.IdEstadoSolicitud == estadoSolicitudId).ToList();
                case 2:  // Director
                    return allItems.Where(i => i.IdUsuarioResponsable == usuario.id_usuario_ci && i.IdDepartamento == usuario.id_departamento && i.IdEstadoSolicitud == estadoSolicitudId).ToList();
                case 3:  // Gerente Area
                    return allItems.Where(i => i.IdUsuarioResponsable == usuario.id_usuario_ci && i.IdSucursal == usuario.id_sucursal && i.IdDepartamento == usuario.id_departamento && i.IdEstadoSolicitud == estadoSolicitudId).ToList();
                case 4:  // Depachador
                    return allItems.Where(i => i.IdSucursal == usuario.id_sucursal && i.IdEstadoSolicitud == estadoSolicitudId).ToList();
                case 5:  // Solicitante
                    return allItems.Where(i => i.CreadoPor == usuario.correo && i.IdDepartamento == usuario.id_departamento && i.IdSucursal == usuario.id_sucursal && i.IdEstadoSolicitud == estadoSolicitudId).ToList();
            }
        }
        return null;
    }

    public Task<List<CabeceraSolicitudCI>> ObtenerSolicitudes()
    {
        var cache = _memoryCache.Get<List<CabeceraSolicitudCI>>("SolicitudesCI");
        if (cache == null)
        {
            cache = _context
                .CabeceraSolicitudesCI.Select(i => new CabeceraSolicitudCI
                {
                    IdCabeceraSolicitud = i.id_cabecera_solicitud,
                    NoDocumento = i.no_documento,
                    FechaCreado = i.fecha_creado,
                    Comentario = i.comentario,
                    CreadoPor = i.creado_por,
                    UsuarioResponsable = i.usuario_responsable ?? "",
                    UsuarioDespacho = i.usuario_despacho ?? "",
                    IdDepartamento = i.id_departamento,
                    IdEstadoSolicitud = i.id_estado_solicitud,
                    IdClasificacion = i.id_clasificacion,
                    IdSucursal = i.id_sucursal,
                    Total = i.total,
                    IdUsuarioResponsable = i.id_usuario_responsable,
                    IdUsuarioDespacho = i.id_usuario_despacho,
                    CantidadLineas = i.LineasSolicitudesCI.Count
                    // FechaModificado = i.fecha_modificado,
                    // ModificadoPor = i.modificado_por,
                })
                .ToList();
            _memoryCache.Set<List<CabeceraSolicitudCI>>(
                "SolicitudesCI",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return Task.FromResult(cache.OrderBy(i => i.IdCabeceraSolicitud.Value).ToList());
    }

    public async Task<CabeceraSolicitudCI> ObtenerSolicitudesPorId(int idSolicitud)
    {
        var allItems = await ObtenerSolicitudes();
        var item = allItems.Where(i => i.IdCabeceraSolicitud == idSolicitud).FirstOrDefault().Clone();
        if (item != null)
        {
            item.Lineas = _context
            .LineasSolicitudesCI.Where(i => i.cabecera_solicitud_id == idSolicitud)
            .Select(i => new LineasSolicitudCI
            {
                IdLineaSolicitud = i.id_linea_solicitud,
                CabeceraSolicitudId = i.cabecera_solicitud_id,
                IdProducto = i.id_producto,
                NoProducto = i.no_producto,
                Descripcion = i.descripcion,
                PrecioUnitario = i.precio_unitario,
                Cantidad = i.cantidad,
                IdUnidadMedida = i.id_unidad_medida,
                CodigoUnidadMedida = i.codigo_unidad_medida,
                // FechaModificado = i.fecha_modificado,
            })
            .OrderBy(i => i.IdLineaSolicitud)
            .ToList();
        }
        return item ?? throw new Exception("Solicitud no encontrada.");
    }

    public async Task<List<CabeceraSolicitudCI>> ObtenerSolicitudesPorEstadoSolicitud(int? estadoSolicitudId)
    {
        var allItems = await ObtenerSolicitudes();
        return allItems.Where(i => i.IdEstadoSolicitud == estadoSolicitudId).ToList();
    }

    public async Task<int> ObtenerCantidadSolicitudesPorEstadoSolicitud(int estadoSolicitudId)
    {
        var allItems = await ObtenerSolicitudes();
        return allItems.Where(i => i.IdEstadoSolicitud == estadoSolicitudId).ToList().Count;
    }

    public async Task<CabeceraSolicitudCI> ObtenerSolicitud(int id)
    {
        var allItems = await ObtenerSolicitudes();
        var item = allItems.Where(i => i.IdCabeceraSolicitud == id).FirstOrDefault().Clone();
        if (item != null)
        {
            item.Lineas = _context
                .LineasSolicitudesCI.Where(i => i.cabecera_solicitud_id == id)
                .Select(i => new LineasSolicitudCI
                {
                    IdLineaSolicitud = i.id_linea_solicitud,
                    CabeceraSolicitudId = i.cabecera_solicitud_id,
                    IdProducto = i.id_producto,
                    NoProducto = i.no_producto,
                    Descripcion = i.descripcion,
                    PrecioUnitario = i.precio_unitario,
                    Cantidad = i.cantidad,
                    IdUnidadMedida = i.id_unidad_medida,
                    CodigoUnidadMedida = i.codigo_unidad_medida,
                    AlmacenId = i.almacen_id,
                    AlmacenCodigo = i.almacen_codigo,
                    Nota = i.nota ?? ""
                })
                .OrderBy(i => i.IdLineaSolicitud)
                .ToList();
        }
        return item;
    }

    public async Task<CabeceraSolicitudCI> GuardarSolicitud(CabeceraSolicitudCI item)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (item.IdCabeceraSolicitud.HasValue)
        {
            // Intentamos obtener el registro para actualizarlo
            var oldItem = await _context.CabeceraSolicitudesCI
                .Where(i => i.id_cabecera_solicitud == item.IdCabeceraSolicitud.Value)
                .FirstOrDefaultAsync();

            if (oldItem == null)
            {
                throw new Exception("El registro no existe para ser actualizado.");
            }

            var cambioEstadoSolicitud = oldItem.id_estado_solicitud != item.IdEstadoSolicitud;
            var datosUsuario = await _context.UsuariosCI.FirstOrDefaultAsync(i => i.correo == identity!.Name);
            var posicion_id = datosUsuario!.posicion_id;
            var usuario_id = datosUsuario!.id_usuario_ci;

            // Actualizamos el objeto existente
            oldItem.fecha_creado = item.FechaCreado;
            oldItem.comentario = item.Comentario;
            oldItem.creado_por = item.CreadoPor;
            oldItem.id_usuario_responsable = item.IdUsuarioResponsable;
            oldItem.usuario_responsable = item.UsuarioResponsable;
            oldItem.id_departamento = item.IdDepartamento;
            oldItem.id_estado_solicitud = item.IdEstadoSolicitud;
            oldItem.id_clasificacion = item.IdClasificacion;
            oldItem.id_sucursal = item.IdSucursal;
            oldItem.total = item.Total;

            if (cambioEstadoSolicitud)
            {
                switch (posicion_id)
                {
                    case 1: //	Administrador
                        switch (item.IdEstadoSolicitud)
                        {
                            case 3: //Aprobada
                                oldItem.id_usuario_responsable = item.IdUsuarioResponsable;
                                oldItem.usuario_responsable = item.UsuarioResponsable;
                                oldItem.id_usuario_despacho = datosUsuario.id_usuario_ci;
                                oldItem.usuario_despacho = datosUsuario.nombre_usuario;
                                break;
                            case 4: //Rechazada
                                oldItem.id_usuario_responsable = item.IdUsuarioResponsable;
                                oldItem.usuario_responsable = item.UsuarioResponsable;
                                oldItem.id_usuario_despacho = datosUsuario.id_usuario_ci;
                                oldItem.usuario_despacho = datosUsuario.nombre_usuario;
                                break;
                            case 5: //Entregada
                                oldItem.id_usuario_despacho = datosUsuario.id_usuario_ci;
                                oldItem.usuario_despacho = datosUsuario.nombre_usuario;
                                break;
                        }
                        break;
                    case 4: //	Depachador
                        oldItem.id_usuario_despacho = datosUsuario.id_usuario_ci;
                        oldItem.usuario_despacho = datosUsuario.nombre_usuario;
                        break;
                }
                oldItem.modificado_por = datosUsuario.nombre_usuario;
                oldItem.fecha_creado = DateTime.Now;
            }

            // Usamos una transacción para asegurar que todo se guarde correctamente
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Guardamos los cambios en la CabeceraSolicitud
                    await _context.SaveChangesAsync();

                    if (cambioEstadoSolicitud)
                    {
                        var buscarNoSerieId = await _context.CabeceraSolicitudesCI.FirstOrDefaultAsync(el => el.id_cabecera_solicitud == item.IdCabeceraSolicitud);

                        // Registramos el historial de la solicitud
                        var nuevoItem = new HistorialMovimientosSolicitudesCI
                        {
                            id_cabecera_solicitud = oldItem.id_cabecera_solicitud,
                            no_serie_id = buscarNoSerieId.no_serie_id,
                            no_documento = item.NoDocumento,
                            fecha_creado = DateTime.Now,
                            creado_por = item.CreadoPor,
                            usuario_responsable = item.UsuarioResponsable,
                            usuario_despacho = item.UsuarioDespacho,
                            id_departamento = item.IdDepartamento,
                            id_estado_solicitud = item.IdEstadoSolicitud,
                            id_clasificacion = item.IdClasificacion,
                            id_sucursal = item.IdSucursal,
                            fecha_modificado = item.FechaModificado,
                            modificado_por = item.ModificadoPor,
                            comentario = item.Comentario,
                            total = item.Total,
                            id_usuario_responsable = item.IdUsuarioResponsable,
                            id_usuario_despacho = item.IdUsuarioDespacho
                        };

                        _context.HistorialMovimientosSolicitudesCI.Add(nuevoItem);

                        // Guardamos los cambios en el historial
                        await _context.SaveChangesAsync();
                    }

                    // Completamos la transacción si todo ha sido exitoso
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    // Si ocurre un error, deshacemos la transacción
                    await transaction.RollbackAsync();
                    throw new Exception("Error al actualizar el registro: " + ex.Message);
                }
            }

            // Calculamos el total de la cabecera actualizada
            CalcularTotalCabecera(oldItem.id_cabecera_solicitud);

            // Limpiamos la caché y la volvemos a poblar
            await RefrescarCache();

            // Retornamos el objeto actualizado
            return await ObtenerSolicitudesPorId(oldItem.id_cabecera_solicitud);
        }
        else
        {
            // Si el IdCabeceraSolicitud no está presente, es una nueva solicitud
            var numeroSerie = await _servicioNumeroSerie.ObtenerNumeroDocumento(_settings.DocumentoConsumoInternoNoSerieId);

            var newItemData = new DataBase.CabeceraSolicitudesCI
            {
                fecha_creado = DateTime.Now,
                comentario = item.Comentario,
                creado_por = identity!.Name,
                no_documento = numeroSerie,
                no_serie_id = _settings.DocumentoConsumoInternoNoSerieId,
                usuario_responsable = item.UsuarioResponsable ?? "",
                usuario_despacho = item.UsuarioDespacho ?? "",
                id_departamento = item.IdDepartamento,
                id_estado_solicitud = item.IdEstadoSolicitud,
                id_clasificacion = item.IdClasificacion,
                id_sucursal = item.IdSucursal,
                id_usuario_responsable = item.IdUsuarioResponsable,
                id_usuario_despacho = item.IdUsuarioDespacho,
                total = item.Total
            };

            var newItem = _context.CabeceraSolicitudesCI.Add(newItemData);

            // Usamos una transacción para asegurar que todo se guarde correctamente
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Guardamos los cambios en la nueva CabeceraSolicitud
                    await _context.SaveChangesAsync();

                    // Registramos el historial de la nueva solicitud
                    var nuevoItem = new HistorialMovimientosSolicitudesCI
                    {
                        id_cabecera_solicitud = newItem.Entity.id_cabecera_solicitud,
                        no_serie_id = _settings.DocumentoConsumoInternoNoSerieId,
                        no_documento = numeroSerie,
                        fecha_creado = DateTime.Now,
                        creado_por = item.CreadoPor,
                        usuario_responsable = item.UsuarioResponsable,
                        usuario_despacho = item.UsuarioDespacho,
                        id_departamento = item.IdDepartamento,
                        id_estado_solicitud = item.IdEstadoSolicitud,
                        id_clasificacion = item.IdClasificacion,
                        id_sucursal = item.IdSucursal,
                        fecha_modificado = item.FechaModificado,
                        modificado_por = item.ModificadoPor,
                        comentario = item.Comentario,
                        total = item.Total,
                        id_usuario_responsable = item.IdUsuarioResponsable,
                        id_usuario_despacho = item.IdUsuarioDespacho,
                    };

                    _context.HistorialMovimientosSolicitudesCI.Add(nuevoItem);

                    // Guardamos los cambios en el historial
                    await _context.SaveChangesAsync();

                    // Completamos la transacción si todo ha sido exitoso
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    // Si ocurre un error, deshacemos la transacción
                    await transaction.RollbackAsync();
                    throw new Exception("Error al crear el registro: " + ex.Message);
                }
            }

            // Calculamos el total de la nueva cabecera
            CalcularTotalCabecera(newItem.Entity.id_cabecera_solicitud);

            // Limpiamos la caché y la volvemos a poblar
            await RefrescarCache();

            // Retornamos el objeto creado
            return await ObtenerSolicitudesPorId(newItem.Entity.id_cabecera_solicitud);
        }
    }

    public async Task<CabeceraSolicitudCI> GuardarLineasSolicitud(List<LineasSolicitudCI> items)
    {
        if (items.Count > 0)
        {
            var parent = _context
                .CabeceraSolicitudesCI.Where(i => i.id_cabecera_solicitud == items[0].CabeceraSolicitudId)
                .FirstOrDefault();

            if (parent != null)
            {
                var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                foreach (var item in items)
                {
                    if (item.IdLineaSolicitud.HasValue)
                    {
                        var oldItem = _context
                            .LineasSolicitudesCI.Where(i =>
                                i.id_linea_solicitud == item.IdLineaSolicitud.Value
                                && i.cabecera_solicitud_id == item.CabeceraSolicitudId
                            )
                            .FirstOrDefault();
                        if (oldItem != null)
                        {
                            oldItem.cabecera_solicitud_id = item.CabeceraSolicitudId;
                            oldItem.id_producto = item.IdProducto;
                            oldItem.no_producto = item.NoProducto;
                            oldItem.descripcion = item.Descripcion;
                            oldItem.precio_unitario = item.PrecioUnitario;
                            oldItem.cantidad = item.Cantidad;
                            oldItem.id_unidad_medida = item.IdUnidadMedida;
                            oldItem.codigo_unidad_medida = item.CodigoUnidadMedida;
                            oldItem.almacen_id = item.AlmacenId;
                            oldItem.almacen_codigo = item.AlmacenCodigo;
                            oldItem.nota = item.Nota ?? "";
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
                        //SE INSERTA EL NUEVO ITEM
                        var newItemData = new LineasSolicitudesCI
                        {
                            cabecera_solicitud_id = item.CabeceraSolicitudId,
                            id_producto = item.IdProducto,
                            no_producto = item.NoProducto,
                            descripcion = item.Descripcion,
                            precio_unitario = item.PrecioUnitario,
                            cantidad = item.Cantidad,
                            id_unidad_medida = item.IdUnidadMedida,
                            codigo_unidad_medida = item.CodigoUnidadMedida,
                            almacen_id = item.AlmacenId,
                            almacen_codigo = item.AlmacenCodigo,
                            nota = item.Nota,
                        };
                        var newItem = _context.LineasSolicitudesCI.Add(newItemData);
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
                }
            }
            else
            {
                throw new Exception("El registro no existe para ser actualizado.");
            }

            //SE ACTUALIZA EL TOTAL DE LA CABECERA
            CalcularTotalCabecera(items[0].CabeceraSolicitudId);

            //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
            await RefrescarCache();

            //SE RETORNA EL OBJETO CREADO
            return await ObtenerSolicitud(items[0].CabeceraSolicitudId);
        }
        else
        {
            throw new Exception(
                string.Format("No se encontró la solicitud {0}", items[0].CabeceraSolicitudId)
            );
        }
    }

    public void CalcularTotalCabecera(int id)
    {
        var item = _context
            .CabeceraSolicitudesCI
            .Where(i => i.id_cabecera_solicitud == id)
            .FirstOrDefault();

        if (item != null)
        {
            item.total = _context
               .LineasSolicitudesCI
               .Where(i => i.cabecera_solicitud_id == id)  // Filtramos por id_cabecera_solicitud
               .Sum(i => i.precio_unitario * i.cantidad); // Sumar el total de las líneas
            _context.SaveChanges();
        }
    }

    public Task<List<CabeceraSolicitudCI>> RecuperarSolicitud(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<CabeceraSolicitudCI> EliminarSolicitud(int id)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        var oldItem = _context.CabeceraSolicitudesCI.Where(i => i.id_cabecera_solicitud == id).FirstOrDefault();
        if (oldItem != null)
        {
            _context.CabeceraSolicitudesCI.Remove(oldItem);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el registro: <" + ex.Message + ">");
            }
            await RefrescarCache();
            return await ObtenerSolicitudesPorId(oldItem.id_cabecera_solicitud);
        }
        return null;
    }

    public async Task<CabeceraSolicitudCI> EliminarLineaSolicitud(int id)
    {
        var oldItem = _context.LineasSolicitudesCI.Where(i => i.id_linea_solicitud == id).FirstOrDefault();
        if (oldItem != null)
        {
            _context.LineasSolicitudesCI.Remove(oldItem);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el registro: <" + ex.Message + ">");
            }
            CalcularTotalCabecera(oldItem.cabecera_solicitud_id);
            await RefrescarCache();
            return await ObtenerSolicitudesPorId(oldItem.cabecera_solicitud_id);
        }
        return null;
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("SolicitudesCI");
        await ObtenerSolicitudes();
        return true;
    }

}