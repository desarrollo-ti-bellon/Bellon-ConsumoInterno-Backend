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
            cache = await _context
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
            .ToListAsync();
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
            cache = await _context
            .HistorialMovimientosSolicitudesCI
            .Where(i => i.creado_por == identity!.Name)
            .GroupBy(i => i.no_documento)
            .Select(g => new HistorialMovimientoSolicitudCI
            {
                NoDocumento = g.Key,
                IdHistMovSolicitud = g.Max(i => i.id_hist_mov_solicitud),
                IdCabeceraSolicitud = g.Max(i => i.id_cabecera_solicitud),
                NoSerieId = g.Max(i => i.no_serie_id),
                FechaCreado = g.Max(i => i.fecha_creado),
                CreadoPor = g.Max(i => i.creado_por) ?? "",
                UsuarioResponsable = g.Max(i => i.usuario_responsable) ?? "",
                UsuarioDespacho = g.Max(i => i.usuario_despacho) ?? "",
                IdDepartamento = g.Max(i => i.id_departamento) ?? "",
                IdEstadoSolicitud = g.Max(i => i.id_estado_solicitud),
                IdClasificacion = g.Max(i => i.id_clasificacion),
                IdSucursal = g.Max(i => i.id_sucursal) ?? "",
                FechaModificado = g.Max(i => i.fecha_modificado),
                ModificadoPor = g.Max(i => i.modificado_por) ?? "",
                Comentario = g.Max(i => i.comentario) ?? "",
                Total = g.Max(i => i.total),
                IdUsuarioResponsable = g.Max(i => i.id_usuario_responsable),
                IdUsuarioDespacho = g.Max(i => i.id_usuario_despacho),
                Indice = g.Max(i => i.indice),
                NombreCreadoPor = g.Max(i => i.nombre_creado_por)
            })
            .ToListAsync();

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

    public async Task<List<HistorialMovimientoSolicitudCI>> ObtenerHistorialMovimientosSolicitudesAgrupadosConFiltrosGenerales(FiltroGeneral filtro)
    {
        // Iniciamos la consulta con el conjunto de datos de 'HistorialMovimientosSolicitudesCI'
        var consulta = _context.HistorialMovimientosSolicitudesCI.AsQueryable();
        var identity = _httpContextAccessor.HttpContext!.User.Identity as ClaimsIdentity;
        var usuario = await _context.UsuariosCI.FirstOrDefaultAsync(el => el.correo == identity!.Name);

        // Aplicar los filtros según los parámetros
        if (filtro.FechaDesde.HasValue)
        {
            var fechaDesde = filtro.FechaDesde.Value.Date;  // Solo la parte de la fecha
            consulta = consulta.Where(i => i.fecha_creado >= fechaDesde);
        }

        if (filtro.FechaHasta.HasValue)
        {
            var fechaHasta = filtro.FechaHasta.Value.Date.AddDays(1).AddMilliseconds(-1);  // Último milisegundo del día
            consulta = consulta.Where(i => i.fecha_creado <= fechaHasta);
        }

        var resultado = await consulta
            .Where(i => i.creado_por == identity!.Name)
            .GroupBy(i => i.no_documento)
            .Select(g => new HistorialMovimientoSolicitudCI
            {
                NoDocumento = g.Key,
                IdHistMovSolicitud = g.Max(i => i.id_hist_mov_solicitud),
                IdCabeceraSolicitud = g.Max(i => i.id_cabecera_solicitud),
                NoSerieId = g.Max(i => i.no_serie_id),
                FechaCreado = g.Max(i => i.fecha_creado),
                CreadoPor = g.Max(i => i.creado_por) ?? "",
                UsuarioResponsable = g.Max(i => i.usuario_responsable) ?? "",
                UsuarioDespacho = g.Max(i => i.usuario_despacho) ?? "",
                IdDepartamento = g.Max(i => i.id_departamento) ?? "",
                IdEstadoSolicitud = g.Max(i => i.id_estado_solicitud),
                IdClasificacion = g.Max(i => i.id_clasificacion),
                IdSucursal = g.Max(i => i.id_sucursal) ?? "",
                FechaModificado = g.Max(i => i.fecha_modificado),
                ModificadoPor = g.Max(i => i.modificado_por) ?? "",
                Comentario = g.Max(i => i.comentario) ?? "",
                Total = g.Max(i => i.total),
                IdUsuarioResponsable = g.Max(i => i.id_usuario_responsable),
                IdUsuarioDespacho = g.Max(i => i.id_usuario_despacho),
                Indice = g.Max(i => i.indice),
                NombreCreadoPor = g.Max(i => i.nombre_creado_por)
            })
            .Distinct()
            .ToListAsync();

        return resultado;
    }
}

