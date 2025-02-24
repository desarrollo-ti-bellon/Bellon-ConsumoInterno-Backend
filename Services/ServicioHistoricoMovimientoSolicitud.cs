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

    public async Task<List<HistorialMovimientoSolicitudCI>> ObtenerHistorialMovimientosSolicitudes()
    {
        var cache = _memoryCache.Get<List<HistorialMovimientoSolicitudCI>>("HistorialMovimientosSolicitudesCI");
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (cache == null)
        {
            cache = _context
            .HistorialMovimientosSolicitudesCI
            .Select(i => new HistorialMovimientoSolicitudCI
            {
                IdHistMovSolicitud = i.id_hist_mov_solicitud,
                IdCabeceraSolicitud = i.id_cabecera_solicitud,
                NoSerieId = i.no_serie_id,
                NoDocumento = i.no_documento,
                FechaCreado = i.fecha_creado,
                CreadoPor = i.creado_por ?? "",
                UsuarioResponsable = i.usuario_responsable ?? "",
                UsuarioDespacho = i.usuario_despacho ?? "",
                IdDepartamento = i.id_departamento,
                IdEstadoSolicitud = i.id_estado_solicitud,
                IdClasificacion = i.id_clasificacion,
                IdSucursal = i.id_sucursal,
                FechaModificado = i.fecha_modificado,
                ModificadoPor = i.modificado_por ?? "",
                Comentario = i.comentario ?? "",
                Total = i.total,
                IdUsuarioResponsable = i.id_usuario_responsable,
                IdUsuarioDespacho = i.id_usuario_despacho,
                Indice = i.indice,
                NombreCreadoPor = i.nombre_creado_por ?? "",
            })
            .Where(i => i.CreadoPor == identity.Name)
            .ToList();
            _memoryCache.Set<List<HistorialMovimientoSolicitudCI>>(
                "HistorialMovimientosSolicitudesCI",
                cache,
                DateTimeOffset.Now.AddMinutes(5)
            );
        }
        return cache.OrderBy(i => i.Indice).ToList();
    }

    public async Task<List<HistorialMovimientoSolicitudCI>> ObtenerHistorialMovimientosSolicitudesAgrupados()
    {
        var cache = _memoryCache.Get<List<HistorialMovimientoSolicitudCI>>("HistorialMovimientosSolicitudesAgrupadosCI");
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (cache == null)
        {
            cache = _context
            .HistorialMovimientosSolicitudesCI
            .Select(i => new HistorialMovimientoSolicitudCI
            {
                IdHistMovSolicitud = i.id_hist_mov_solicitud,
                IdCabeceraSolicitud = i.id_cabecera_solicitud,
                NoSerieId = i.no_serie_id,
                NoDocumento = i.no_documento,
                FechaCreado = i.fecha_creado,
                CreadoPor = i.creado_por ?? "",
                UsuarioResponsable = i.usuario_responsable ?? "",
                UsuarioDespacho = i.usuario_despacho ?? "",
                IdDepartamento = i.id_departamento,
                IdEstadoSolicitud = i.id_estado_solicitud,
                IdClasificacion = i.id_clasificacion,
                IdSucursal = i.id_sucursal,
                FechaModificado = i.fecha_modificado,
                ModificadoPor = i.modificado_por ?? "",
                Comentario = i.comentario ?? "",
                Total = i.total,
                IdUsuarioResponsable = i.id_usuario_responsable,
                IdUsuarioDespacho = i.id_usuario_despacho
            })
            .Where(i => i.CreadoPor == identity.Name)
            .GroupBy(i => i.NoDocumento)
            .Select(g => g.OrderByDescending(i => i.FechaCreado).First())
            .ToList();
            _memoryCache.Set<List<HistorialMovimientoSolicitudCI>>(
                "HistorialMovimientosSolicitudesAgrupadosCI",
                cache,
                DateTimeOffset.Now.AddMinutes(5)
            );
        }
        return cache.OrderBy(i => i.Indice).ToList();
    }

    public async Task<List<HistorialMovimientoSolicitudCI>> ObtenerHistorialMovimientosSolicitudes(string documento)
    {
        var allItems = await ObtenerHistorialMovimientosSolicitudes();
        var items = allItems.Where(i => i.NoDocumento == documento).ToList();
        return items;
    }

    public async Task<HistorialMovimientoSolicitudCI> ObtenerHistorialMovimientoSolicitud(int? id)
    {
        var allItems = await ObtenerHistorialMovimientosSolicitudes();
        var items = allItems.Where(i => i.IdCabeceraSolicitud == id).FirstOrDefault().Clone();
        return items;
    }

    public async Task<int> ObtenerCantidadHistorialMovimientoSolicitud()
    {
        var allItems = await ObtenerHistorialMovimientosSolicitudes();
        return allItems.Count();
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("HistorialMovimientosSolicitudesCI");
        _memoryCache.Remove("HistorialMovimientosSolicitudesAgrupadosCI");
        await ObtenerHistorialMovimientosSolicitudes();
        return true;
    }

}

