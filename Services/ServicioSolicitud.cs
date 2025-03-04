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
    private readonly IServicioAjusteInventario _servicioAjusteInventario;

    public ServicioSolicitud(
        IHttpContextAccessor httpContextAccessor,
        AppDataBase context,
        IOptions<AppSettings> settings,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IServicioAutorizacion servicioAutorizacion,
        IServicioNumeroSerie servicioNumeroSerie,
        IServicioAjusteInventario servicioAjusteInventario
    )
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _servicioAutorizacion = servicioAutorizacion;
        _servicioNumeroSerie = servicioNumeroSerie;
        _servicioAjusteInventario = servicioAjusteInventario;
    }

    //ESTADOS DE SOLICITUDES 
    public int estadoSolicitudNueva = 1;
    public int estadoSolicitudPendiente = 2;
    public int estadoSolicitudAprobada = 3;
    public int estadoSolicitudRechazada = 4;
    public int estadoSolicitudEntregada = 5;
    public int estadoSolicitudConfirmada = 6;
    public int estadoSolicitudTerminada = 7;

    public async Task<List<CabeceraSolicitudCI>> ObtenerSolicitudesPorPerfilUsuario()
    {
        // Obtener todas las solicitudes solo una vez
        var allItems = await ObtenerSolicitudes();

        // Obtener el usuario actual desde la identidad
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (identity == null)
        {
            return new List<CabeceraSolicitudCI>();  // Retornar una lista vacía si no hay identidad
        }

        // Obtener el usuario desde la base de datos
        var usuario = await _context.UsuariosCI.FirstOrDefaultAsync(el => el.correo == identity.Name);
        if (usuario == null)
        {
            return new List<CabeceraSolicitudCI>();  // Retornar una lista vacía si no se encuentra el usuario
        }

        // Definir el diccionario de roles y sus estados de solicitudes
        var estadosPorPerfil = new Dictionary<int, List<int>>()
        {
            { 1, new List<int> { estadoSolicitudNueva, estadoSolicitudPendiente, estadoSolicitudAprobada, estadoSolicitudRechazada, estadoSolicitudEntregada, estadoSolicitudConfirmada, estadoSolicitudTerminada } },  // Administrador
            { 2, new List<int> { estadoSolicitudPendiente } },                                                                                                                                                          // Director
            { 3, new List<int> { estadoSolicitudPendiente } },                                                                                                                                                          // Gerente Area
            { 4, new List<int> { estadoSolicitudAprobada } },                                                                                                                                                           // Despachador
            { 5, new List<int> { estadoSolicitudNueva, estadoSolicitudAprobada, estadoSolicitudRechazada, estadoSolicitudEntregada } }                                                                                  // Solicitante
        };

        // Verificar que el perfil del usuario exista en el diccionario
        if (!estadosPorPerfil.ContainsKey(usuario.posicion_id))
        {
            return new List<CabeceraSolicitudCI>();  // Retornar una lista vacía si el perfil no está en el diccionario
        }

        // Filtrar las solicitudes dependiendo del perfil
        var arrEstadosSolicitudes = estadosPorPerfil[usuario.posicion_id];

        var query = allItems;
        var resultado = new List<CabeceraSolicitudCI>();
        switch (usuario.posicion_id)
        {
            case 1: // Administrador
                resultado = query.Where(i => i.IdSucursal == usuario.id_sucursal && arrEstadosSolicitudes.Contains(i.IdEstadoSolicitud)).ToList();
                break;

            case 2: // Director
                resultado = query.Where(i => i.IdUsuarioResponsable == usuario.id_usuario_ci && i.IdDepartamento == usuario.id_departamento && arrEstadosSolicitudes.Contains(i.IdEstadoSolicitud)).ToList();
                break;

            case 3: // Gerente Area
                resultado = query.Where(i => i.IdUsuarioResponsable == usuario.id_usuario_ci && i.IdSucursal == usuario.id_sucursal && i.IdDepartamento == usuario.id_departamento && arrEstadosSolicitudes.Contains(i.IdEstadoSolicitud)).ToList();
                break;

            case 4: // Despachador
                resultado = query.Where(i => i.IdSucursal == usuario.id_sucursal && arrEstadosSolicitudes.Contains(i.IdEstadoSolicitud)).ToList();
                break;

            case 5: // Solicitante
                resultado = query.Where(i => i.CreadoPor == usuario.correo && i.IdDepartamento == usuario.id_departamento && i.IdSucursal == usuario.id_sucursal && arrEstadosSolicitudes.Contains(i.IdEstadoSolicitud)).ToList();
                break;
        }

        // Obtener los resultados filtrados
        return resultado;
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
                    return allItems.Where(i => i.IdEstadoSolicitud == estadoSolicitudId).ToList();
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
                    CantidadLineas = i.LineasSolicitudesCI.Count,
                    NombreCreadoPor = i.nombre_creado_por
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
                Total = i.total,
                CostoUnitario = i.costo_unitario,
            })
            .OrderBy(i => i.IdLineaSolicitud)
            .ToList();
        }
        return item;
    }

    public async Task<List<CabeceraSolicitudCI>> ObtenerSolicitudesPorEstadoSolicitud(int? estadoSolicitudId)
    {
        var allItems = await ObtenerSolicitudes();
        return allItems.Where(i => i.IdEstadoSolicitud == estadoSolicitudId).ToList();
    }

    public async Task<int> ObtenerCantidadSolicitudesPorEstadoSolicitud(int estadoSolicitudId)
    {
        var allItems = await ObtenerSolicitudes();
        var identity = _httpContextAccessor.HttpContext!.User.Identity as ClaimsIdentity;
        var usuario = await _context.UsuariosCI.FirstOrDefaultAsync(el => el.correo == identity!.Name);
        var resultado = new List<CabeceraSolicitudCI>();

        switch (usuario.posicion_id)
        {
            case 1: // Administrador
                resultado = allItems.ToList();
                break;

            case 2: // Director
                resultado = allItems.Where(i => i.IdUsuarioResponsable == usuario.id_usuario_ci && i.IdDepartamento == usuario.id_departamento).ToList();
                break;

            case 3: // Gerente Area
                resultado = allItems.Where(i => i.IdUsuarioResponsable == usuario.id_usuario_ci && i.IdSucursal == usuario.id_sucursal && i.IdDepartamento == usuario.id_departamento).ToList();
                break;

            case 4: // Despachador
                resultado = allItems.Where(i => i.IdSucursal == usuario.id_sucursal).ToList();
                break;

            case 5: // Solicitante
                resultado = allItems.Where(i => i.CreadoPor == usuario.correo && i.IdDepartamento == usuario.id_departamento && i.IdSucursal == usuario.id_sucursal).ToList();
                break;
        }

        return resultado.Where(i => i.IdEstadoSolicitud == estadoSolicitudId).ToList().Count();
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
                    Nota = i.nota ?? "",
                    Total = i.total,
                    CostoUnitario = i.costo_unitario,
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
            oldItem.modificado_por = identity!.Name;
            oldItem.fecha_modificado = DateTime.Now;
            oldItem.id_usuario_responsable = item.IdUsuarioResponsable;
            oldItem.usuario_responsable = item.UsuarioResponsable;
            oldItem.id_departamento = item.IdDepartamento;
            oldItem.id_estado_solicitud = item.IdEstadoSolicitud;
            oldItem.id_clasificacion = item.IdClasificacion;
            oldItem.id_sucursal = item.IdSucursal;
            oldItem.total = item.Total;
            oldItem.nombre_creado_por = item.NombreCreadoPor;

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

                        // GUARDAMOS EL HISTORICO DEL ESTADO DE LA SOLICITUD
                        await GuardarHistoricoSolicitudes(item);

                        // AQUI SE AGREGAN LAS SOLICITUDES AL CONSUMO INTERNO CUANDO TERMINA EL PROCESO CORRECTAMENTE EN EL LS CENTRAL
                        if (item.IdEstadoSolicitud == 6) // SOLICITUD CONFIRMADA 
                        {

                            var verificarArchivado = await Archivar(oldItem.id_cabecera_solicitud);
                            if (!verificarArchivado.Exito)
                            {
                                await transaction.RollbackAsync();
                                throw new Exception("Error al archivar la solicitud, transacción revertida.");
                            }

                            var seHizoAjusteInventario = await _servicioAjusteInventario.CrearAjusteInventario(oldItem.id_cabecera_solicitud);
                            if (!seHizoAjusteInventario.Exito)
                            {
                                await transaction.RollbackAsync();
                                throw new Exception(seHizoAjusteInventario.Mensaje);
                            }

                        }

                    }
                    else
                    {
                        // GUARDAMOS EL HISTORICO DEL ESTADO DE LA SOLICITUD
                        await GuardarHistoricoSolicitudes(item);
                    }

                    // Completamos la transacción si todo ha sido exitoso
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    // Si ocurre un error, deshacemos la transacción
                    await transaction.RollbackAsync();
                    throw new Exception("Error al actualizar el registro: " + ex.Message, ex);
                }
            }

            // Calculamos el total de la cabecera actualizada
            CalcularTotalCabecera(oldItem.id_cabecera_solicitud);
            CalcularTotalHistorial(oldItem.id_cabecera_solicitud);

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
                total = 0,
                nombre_creado_por = item.NombreCreadoPor
            };

            var newItem = _context.CabeceraSolicitudesCI.Add(newItemData);

            // Usamos una transacción para asegurar que todo se guarde correctamente
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Guardamos los cambios en la nueva CabeceraSolicitud
                    await _context.SaveChangesAsync();

                    // GUARDAMOS EL HISTORICO DEL ESTADO DE LA SOLICITUD
                    item.NoDocumento = numeroSerie;
                    item.IdCabeceraSolicitud = newItem.Entity.id_cabecera_solicitud;
                    item.Total = 0;
                    await GuardarHistoricoSolicitudes(item);

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
            CalcularTotalHistorial(newItem.Entity.id_cabecera_solicitud);

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
                            oldItem.total = item.Cantidad * item.PrecioUnitario;
                            oldItem.costo_unitario = item.CostoUnitario;
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
                            total = item.Cantidad * item.PrecioUnitario,
                            costo_unitario = item.CostoUnitario,
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
            CalcularTotalHistorial(items[0].CabeceraSolicitudId);
            _memoryCache.Remove("HistorialMovimientosSolicitudesCI");
            _memoryCache.Remove("HistorialMovimientosSolicitudesAgrupadosCI");

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
            CalcularTotalHistorial(oldItem.cabecera_solicitud_id);
            await RefrescarCache();
            return await ObtenerSolicitudesPorId(oldItem.cabecera_solicitud_id);
        }
        return null;
    }

    public async Task<Resultado> Archivar(int id)
    {
        var item = await ObtenerSolicitud(id);
        if (item == null)
        {
            return new Resultado
            {
                Exito = false,
                Mensaje = $"No se encontró la Solicitud {id} para archivar"
            };
        }

        // Usamos una transacción para asegurar que todo se haga de manera atómica
        try
        {
            // CREANDO ENCABEZADO DE CONSUMO INTERNO
            var buscarNoSerieId = await _context.CabeceraSolicitudesCI.FirstOrDefaultAsync(el => el.id_cabecera_solicitud == item.IdCabeceraSolicitud);
            var consumoInterno = new CabeceraConsumosInternos
            {
                id_cabecera_consumo_interno = item.IdCabeceraSolicitud.Value,
                no_serie_id = buscarNoSerieId.no_serie_id,
                no_documento = item.NoDocumento ?? "",
                fecha_creado = item.FechaCreado,
                creado_por = item.CreadoPor,
                usuario_responsable = item.UsuarioResponsable,
                usuario_despacho = item.UsuarioDespacho ?? "",
                id_departamento = item.IdDepartamento,
                id_estado_solicitud = estadoSolicitudTerminada,
                id_clasificacion = item.IdClasificacion,
                id_sucursal = item.IdSucursal,
                fecha_modificado = item.FechaModificado,
                modificado_por = item.ModificadoPor ?? "",
                comentario = item.Comentario ?? "",
                total = item.Total,
                id_usuario_responsable = item.IdUsuarioResponsable,
                id_usuario_despacho = item.IdUsuarioDespacho,
                nombre_creado_por = item.NombreCreadoPor
            };

            // Habilitar el insert explícito para poder insertar el ID
            _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [ConsumoInterno].[CabeceraConsumosInternos] ON");
            _context.CabeceraConsumosInternos.Add(consumoInterno);
            await _context.SaveChangesAsync();
            _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [ConsumoInterno].[CabeceraConsumosInternos] OFF");

            // COPIANDO LAS LINEAS DE SOLICITUD A LA TABLA DE CONSUMOS INTERNOS
            var lineasSolicitudesCI = item.Lineas.Where(i => i.CabeceraSolicitudId == id).ToList();
            if (lineasSolicitudesCI.Any())
            {
                // Verificar si el id_cabecera_consumo_interno ya existe
                bool existeCabeceraConsumo = await _context.CabeceraConsumosInternos
                    .AnyAsync(c => c.id_cabecera_consumo_interno == consumoInterno.id_cabecera_consumo_interno);

                if (!existeCabeceraConsumo)
                {
                    return new Resultado
                    {
                        Exito = false,
                        Mensaje = $"No existe un consumo interno con el ID {consumoInterno.id_cabecera_consumo_interno}"
                    };
                }

                // Agregar las líneas de consumo en bloque
                var lineasConsumosInternos = lineasSolicitudesCI.Select(linea => new LineasConsumosInternos
                {
                    cabecera_consumo_interno_id = consumoInterno.id_cabecera_consumo_interno,
                    id_producto = linea.IdProducto,
                    id_unidad_medida = linea.IdUnidadMedida,
                    cantidad = linea.Cantidad,
                    precio_unitario = linea.PrecioUnitario,
                    no_producto = linea.NoProducto,
                    descripcion = linea.Descripcion,
                    codigo_unidad_medida = linea.CodigoUnidadMedida,
                    almacen_id = linea.AlmacenId ?? "",
                    almacen_codigo = linea.AlmacenCodigo ?? "",
                    nota = linea.Nota ?? "",
                    total = linea.Total
                }).ToList();

                _context.LineasConsumosInternos.AddRange(lineasConsumosInternos);
                await _context.SaveChangesAsync();
            }

            item.IdEstadoSolicitud = estadoSolicitudTerminada;
            await GuardarHistoricoSolicitudes(item);

            // SE ELIMINAN LOS DETALLES DE LA TABLA DE PRODUCCIÓN
            var itemDB = await _context.CabeceraSolicitudesCI
                .FirstOrDefaultAsync(i => i.id_cabecera_solicitud == id);
            if (itemDB != null)
            {
                _context.CabeceraSolicitudesCI.Remove(itemDB);
            }

            var detailsDB = _context.LineasSolicitudesCI.Where(i => i.cabecera_solicitud_id == id);
            _context.LineasSolicitudesCI.RemoveRange(detailsDB);

            // GUARDAR CAMBIOS Y COMMIT DE LA TRANSACCIÓN
            await _context.SaveChangesAsync();

            // SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
            await RefrescarCache();
            _memoryCache.Remove("SolicitudesCI");
            _memoryCache.Remove("HistorialMovimientosSolicitudesCI");
            _memoryCache.Remove("HistorialMovimientosSolicitudesAgrupadosCI");
            _memoryCache.Remove("ConsumosInternos");

            return new Resultado { Exito = true };
        }
        catch (Exception ex)
        {
            // Loguear el error si es necesario
            return new Resultado
            {
                Exito = false,
                Mensaje = "Ocurrió un error al archivar la solicitud: " + ex.Message
            };
        }
    }

    public async Task<bool> GuardarHistoricoSolicitudes(CabeceraSolicitudCI item)
    {
        // Registramos el historial de la solicitud
        var oldItem = await _context.HistorialMovimientosSolicitudesCI
                        .Where(i => i.id_cabecera_solicitud == item.IdCabeceraSolicitud)
                        .FirstOrDefaultAsync();

        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (
            oldItem != null
            && item.IdEstadoSolicitud == estadoSolicitudNueva // SOLO SI ES NUEVO 
        )
        {
            try
            {
                // Registramos el historial de la nueva solicitud
                oldItem.id_cabecera_solicitud = item.IdCabeceraSolicitud;
                oldItem.no_serie_id = _settings.DocumentoConsumoInternoNoSerieId;
                oldItem.no_documento = item.NoDocumento;
                oldItem.fecha_creado = item.FechaCreado;
                oldItem.creado_por = item.CreadoPor;
                oldItem.usuario_responsable = item.UsuarioResponsable;
                oldItem.usuario_despacho = item.UsuarioDespacho;
                oldItem.id_departamento = item.IdDepartamento;
                oldItem.id_estado_solicitud = item.IdEstadoSolicitud;
                oldItem.id_clasificacion = item.IdClasificacion;
                oldItem.id_sucursal = item.IdSucursal;
                oldItem.comentario = item.Comentario;
                oldItem.total = item.Total;
                oldItem.id_usuario_responsable = item.IdUsuarioResponsable;
                oldItem.id_usuario_despacho = item.IdUsuarioDespacho;
                oldItem.nombre_creado_por = item.NombreCreadoPor;
                oldItem.fecha_modificado = DateTime.Now;
                oldItem.modificado_por = identity!.Name;
                oldItem.indice = 1;

                _memoryCache.Remove("HistorialMovimientosSolicitudesAgrupadosCI");
                _memoryCache.Remove("HistorialMovimientosSolicitudesCI");
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al intentar guardar el historico del registro: <" + ex.Message + ">");
            }
        }
        else
        {
            try
            {
                var ultimoHistorial = _context.HistorialMovimientosSolicitudesCI
                                   .OrderByDescending(i => i.fecha_creado) // Aseguramos que estamos obteniendo el más reciente.
                                   .Where(i => i.id_cabecera_solicitud == item.IdCabeceraSolicitud)
                                   .FirstOrDefault().Clone();

                int nuevoIndice = ultimoHistorial?.indice + 1 ?? 1;

                // Registramos el historial de la nueva solicitud
                var nuevoItem = new HistorialMovimientosSolicitudesCI
                {
                    id_cabecera_solicitud = item.IdCabeceraSolicitud,
                    no_serie_id = _settings.DocumentoConsumoInternoNoSerieId,
                    no_documento = item.NoDocumento,
                    fecha_creado = item.FechaCreado,
                    creado_por = item.CreadoPor,
                    usuario_responsable = item.UsuarioResponsable,
                    usuario_despacho = item.UsuarioDespacho,
                    id_departamento = item.IdDepartamento,
                    id_estado_solicitud = item.IdEstadoSolicitud,
                    id_clasificacion = item.IdClasificacion,
                    id_sucursal = item.IdSucursal,
                    comentario = item.Comentario,
                    total = item.Total,
                    id_usuario_responsable = item.IdUsuarioResponsable,
                    id_usuario_despacho = item.IdUsuarioDespacho,
                    nombre_creado_por = item.NombreCreadoPor,
                    fecha_modificado = DateTime.Now,
                    modificado_por = identity!.Name,
                    indice = nuevoIndice
                };

                _context.HistorialMovimientosSolicitudesCI.Add(nuevoItem);
                _memoryCache.Remove("HistorialMovimientosSolicitudesAgrupadosCI");
                _memoryCache.Remove("HistorialMovimientosSolicitudesCI");
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al intentar guardar el historico del registro: <" + ex.Message + ">");
            }
        }
    }

    public void CalcularTotalHistorial(int id)
    {
        var item = _context
            .HistorialMovimientosSolicitudesCI
            .Where(i => i.id_cabecera_solicitud == id && i.id_estado_solicitud == estadoSolicitudNueva)
            .FirstOrDefault();

        if (item != null)
        {
            item.total = _context
               .LineasSolicitudesCI
               .Where(i => i.cabecera_solicitud_id == id) // Filtramos por id_cabecera_solicitud
               .Sum(i => i.precio_unitario * i.cantidad); // Sumar el total de las líneas
            _context.SaveChanges();
        }
    }

    public async Task<CabeceraSolicitudCI> VerSolicitudesGenerales(int id)
    {
        var solicitud = await _context.CabeceraSolicitudesCI
            .Where(s => s.id_cabecera_solicitud == id)
            .Select(s => new CabeceraSolicitudCI
            {
                IdCabeceraSolicitud = s.id_cabecera_solicitud,
                NoDocumento = s.no_documento,
                FechaCreado = s.fecha_creado,
                Comentario = s.comentario,
                CreadoPor = s.creado_por,
                UsuarioResponsable = s.usuario_responsable ?? "",
                UsuarioDespacho = s.usuario_despacho ?? "",
                IdDepartamento = s.id_departamento,
                IdEstadoSolicitud = s.id_estado_solicitud,
                IdClasificacion = s.id_clasificacion,
                IdSucursal = s.id_sucursal,
                Total = s.total,
                IdUsuarioResponsable = s.id_usuario_responsable,
                IdUsuarioDespacho = s.id_usuario_despacho,
                CantidadLineas = s.LineasSolicitudesCI.Count,
                NombreCreadoPor = s.nombre_creado_por ?? "",
                Lineas = s.LineasSolicitudesCI.Select(d => new LineasSolicitudCI
                {
                    IdLineaSolicitud = d.id_linea_solicitud,
                    CabeceraSolicitudId = d.cabecera_solicitud_id,
                    IdProducto = d.id_producto,
                    NoProducto = d.no_producto,
                    Descripcion = d.descripcion,
                    PrecioUnitario = d.precio_unitario,
                    Cantidad = d.cantidad,
                    IdUnidadMedida = d.id_unidad_medida,
                    CodigoUnidadMedida = d.codigo_unidad_medida,
                    AlmacenId = d.almacen_id,
                    AlmacenCodigo = d.almacen_codigo,
                    Total = d.total,
                    Nota = d.nota
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (solicitud == null)
        {
            solicitud = await _context.CabeceraConsumosInternos
          .Where(s => s.id_cabecera_consumo_interno == id)
          .Select(s => new CabeceraSolicitudCI
          {
              IdCabeceraSolicitud = s.id_cabecera_consumo_interno,
              NoDocumento = s.no_documento,
              FechaCreado = s.fecha_creado,
              Comentario = s.comentario,
              CreadoPor = s.creado_por,
              UsuarioResponsable = s.usuario_responsable ?? "",
              UsuarioDespacho = s.usuario_despacho ?? "",
              IdDepartamento = s.id_departamento,
              IdEstadoSolicitud = s.id_estado_solicitud,
              IdClasificacion = s.id_clasificacion,
              IdSucursal = s.id_sucursal,
              Total = s.total,
              IdUsuarioResponsable = s.id_usuario_responsable,
              IdUsuarioDespacho = s.id_usuario_despacho,
              CantidadLineas = s.LineasConsumosInternos.Count,
              NombreCreadoPor = s.nombre_creado_por ?? "",
              Lineas = s.LineasConsumosInternos.Select(d => new LineasSolicitudCI
              {
                  IdLineaSolicitud = d.id_linea_consumo_interno,
                  CabeceraSolicitudId = d.cabecera_consumo_interno_id,
                  IdProducto = d.id_producto,
                  NoProducto = d.no_producto,
                  Descripcion = d.descripcion,
                  PrecioUnitario = d.precio_unitario,
                  Cantidad = d.cantidad,
                  IdUnidadMedida = d.id_unidad_medida,
                  CodigoUnidadMedida = d.codigo_unidad_medida,
                  AlmacenId = d.almacen_id,
                  AlmacenCodigo = d.almacen_codigo,
                  Total = d.total,
                  Nota = d.nota
              }).ToList()
          })
          .FirstOrDefaultAsync();
        }

        return solicitud;
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("SolicitudesCI");
        await ObtenerSolicitudes();
        return true;
    }

}