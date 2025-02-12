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

public class ServicioHistoricoMovimientoSolicitud : IServicioHistorialMovimientosSolicitudes
{
    private readonly AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;
    private readonly IServicioNumeroSerie _servicioNumeroSerie;

    public ServicioHistoricoMovimientoSolicitud(
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

    public Task<List<HistorialMovimientoSolicitud>> ObtenerHistorialMovimientosSolicitudes()
    {
        var cache = _memoryCache.Get<List<HistorialMovimientoSolicitud>>("HistorialMovimientosSolicitudes");
        if (cache == null)
        {
            cache = _context
                .HistorialMovimientosSolicitudes.Select(i => new HistorialMovimientoSolicitud
                {
                    IdCabeceraSolicitud = i.id_cabecera_solicitud,
                    NoDocumento = i.no_documento,
                    FechaCreado = i.fecha_creado,
                    Comentario = i.comentario,
                    CreadoPor = i.creado_por,
                    UsuarioResponsable = i.usuario_responsable ?? "",
                    UsuarioDespacho = i.usuario_despacho ?? "",
                    UsuarioAsistenteInventario = i.usuario_asistente_inventario ?? "",
                    UsuarioAsistenteContabilidad = i.usuario_asistente_contabilidad ?? "",
                    IdDepartamento = i.id_departamento,
                    IdEstadoSolicitud = i.id_estado_solicitud,
                    IdClasificacion = i.id_clasificacion,
                    IdSucursal = i.id_sucursal,
                    Total = i.total,
                    IdUsuarioResponsable = i.id_usuario_responsable,
                    IdUsuarioDespacho = i.id_usuario_despacho,
                    IdUsuarioAsistenteInventario = i.id_usuario_asistente_inventario,
                    IdUsuarioAsistenteContabilidad = i.id_usuario_asistente_contabilidad,
                    // FechaModificado = i.fecha_modificado,
                    // ModificadoPor = i.modificado_por,
                })
                .ToList();
            _memoryCache.Set<List<HistorialMovimientoSolicitud>>(
                "HistorialMovimientosSolicitudes",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return Task.FromResult(cache.OrderBy(i => i.IdCabeceraSolicitud.Value).ToList());
    }

    public async Task<List<HistorialMovimientoSolicitud>> ObtenerHistorialMovimientoSolicitud(int? id)
    {
        var allItems = await ObtenerHistorialMovimientosSolicitudes();
        return allItems.Where(i => i.IdCabeceraSolicitud == id).OrderBy(i => i.FechaCreado).ToList();
    }

    public async Task<List<HistorialMovimientoSolicitud>> GuardarHistorialMovimientoSolicitud(HistorialMovimientoSolicitud item)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        var newItemData = new DataBase.HistorialMovimientosSolicitudes
        {
            fecha_creado = DateTime.Now,
            comentario = item.Comentario,
            creado_por = identity!.Name,
            no_serie_id = _settings.DocumentoConsumoInternoNoSerieId,
            usuario_responsable = item.UsuarioResponsable ?? "",
            usuario_despacho = item.UsuarioDespacho ?? "",
            usuario_asistente_contabilidad = item.UsuarioAsistenteContabilidad ?? "",
            usuario_asistente_inventario = item.UsuarioAsistenteInventario ?? "",
            id_departamento = item.IdDepartamento,
            id_estado_solicitud = item.IdEstadoSolicitud,
            id_clasificacion = item.IdClasificacion,
            id_sucursal = item.IdSucursal,
            id_usuario_responsable = item.IdUsuarioResponsable,
            id_usuario_despacho = item.IdUsuarioDespacho,
            id_usuario_asistente_inventario = item.IdUsuarioAsistenteInventario,
            id_usuario_asistente_contabilidad = item.IdUsuarioAsistenteContabilidad,
            total = item.Total
        };

        var newItem = _context.HistorialMovimientosSolicitudes.Add(newItemData);
        try
        {
            _context.SaveChanges();
        }
        catch (SqlException sqlEx)
        {
            // Manejo específico de error SQL
            throw new Exception("Error de base de datos: " + sqlEx.Message);
        }
        catch (Exception ex)
        {
            throw new Exception("Error al crear el registro: <" + ex.Message + ">");
        }

        //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
        await RefrescarCache();

        var id = newItem.Entity.id_cabecera_solicitud.HasValue ? newItem.Entity.id_cabecera_solicitud : 0;

        //SE RETORNA EL OBJETO CREADO
        return await ObtenerHistorialMovimientoSolicitud(id);

    }

    public Task<List<HistorialMovimientoSolicitud>> EliminarHistorialMovimientoSolicitud(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("HistorialMovimientosSolicitudes");
        await ObtenerHistorialMovimientosSolicitudes();
        return true;
    }

}

