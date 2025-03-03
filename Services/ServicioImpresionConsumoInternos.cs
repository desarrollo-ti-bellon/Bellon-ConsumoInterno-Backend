using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using Bellon.API.ConsumoInterno.Classes;
using Bellon.API.ConsumoInterno.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Bellon.API.ConsumoInterno.Services;

public class ServicioImpresionConsumoInternos : IServicioImpresionConsumoInterno
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    private DateTime fechaLimite;

    public ServicioImpresionConsumoInternos(
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
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _servicioAutorizacion = servicioAutorizacion;
        fechaLimite = DateTime.Now.AddDays(-3);
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("ImpresionConsumosInternos");
        await ObtenerImpresionConsumosInternos();
        return true;
    }

    public async Task<List<ImpresionConsumoInterno>> ObtenerImpresionConsumosInternos()
    {
        var cache = _memoryCache.Get<List<ImpresionConsumoInterno>>("ImpresionConsumosInternos");
        if (cache == null)
        {

            cache = await _context
                .ImpresionConsumosInternos.Select(i => new ImpresionConsumoInterno
                {
                    IdProducto = i.id_producto,
                    NoProducto = i.no_producto,
                    NoDocumento = i.no_documento,
                    Descripcion = i.descripcion,
                    FechaCreado = i.fecha_creado,
                    AlmacenCodigo = i.almacen_codigo,
                    AlmacenId = i.almacen_id,
                    IdClasificacion = i.id_clasificacion,
                    ClasificacionDescripcion = i.clasificacion_descripcion,
                    CantidadTotal = i.cantidad_total,
                    PrecioUnitarioTotal = i.precio_unitario_total,
                    Total = i.total,
                })
                .ToListAsync();

            _memoryCache.Set<List<ImpresionConsumoInterno>>(
                "ImpresionConsumosInternos",
                cache,
                DateTimeOffset.Now.AddMinutes(5)
            );

        }
        return cache;
    }

    public async Task<List<ImpresionConsumoInterno>> ObtenerImpresionConsumosInternosConFiltros(FiltroGeneral filtro)
    {
        // Iniciamos la consulta con el conjunto de datos de 'HistorialMovimientosSolicitudesCI'
        var consulta = _context.ImpresionConsumosInternos.AsQueryable();
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        var usuario = await _context.UsuariosCI.FirstOrDefaultAsync(el => el.correo == identity!.Name);

        // Aplicar los filtros según los parámetros
        if (!string.IsNullOrEmpty(filtro.NoDocumento))
        {
            consulta = consulta.Where(i => i.no_documento == filtro.NoDocumento);
        }

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

        var resultado = await consulta.Select(i => new ImpresionConsumoInterno
        {
            IdProducto = i.id_producto,
            NoProducto = i.no_producto,
            NoDocumento = i.no_documento,
            Descripcion = i.descripcion,
            FechaCreado = i.fecha_creado,
            AlmacenCodigo = i.almacen_codigo,
            AlmacenId = i.almacen_id,
            IdClasificacion = i.id_clasificacion,
            ClasificacionDescripcion = i.clasificacion_descripcion,
            CantidadTotal = i.cantidad_total,
            PrecioUnitarioTotal = i.precio_unitario_total,
            Total = i.total,
        }).ToListAsync();

        return resultado;
    }

}
