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

public class ServicioConsumoInterno : IServicioConsumoInterno
{
    private readonly AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;
    private readonly IServicioNumeroSerie _servicioNumeroSerie;

    public ServicioConsumoInterno(
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

    public async Task<List<CabeceraConsumoInterno>> ObtenerConsumosInternosSegunPosicionUsuarioYFiltrosGenerales(FiltroGeneral filtro)
    {

        if (filtro == null)
        {
            throw new InvalidDataException("El filtro no puede ser nulo.");
        }

        // Iniciamos la consulta con el conjunto de datos de 'HistorialMovimientosSolicitudesCI'
        var consulta = _context.CabeceraConsumosInternos.AsQueryable();
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        var usuario = await _context.UsuariosCI.FirstOrDefaultAsync(el => el.correo == identity!.Name);

        // Aplicar los filtros según los parámetros
        if (!string.IsNullOrEmpty(filtro.NoDocumento))
        {
            consulta = consulta.Where(i => i.no_documento == filtro.NoDocumento);
        }

        if (!string.IsNullOrEmpty(filtro.CreadoPor))
        {
            consulta = consulta.Where(i => i.creado_por == filtro.CreadoPor);
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

        if (!string.IsNullOrEmpty(filtro.UsuarioResponsable))
        {
            consulta = consulta.Where(i => i.usuario_responsable == filtro.UsuarioResponsable);
        }

        if (filtro.EstadoSolicitudId.HasValue)
        {
            consulta = consulta.Where(i => i.id_estado_solicitud == filtro.EstadoSolicitudId);
        }

        if (!string.IsNullOrEmpty(filtro.IdSucursal))
        {
            consulta = consulta.Where(i => i.id_sucursal == filtro.IdSucursal);
        }

        if (!string.IsNullOrEmpty(filtro.IdDepartamento))
        {
            consulta = consulta.Where(i => i.id_departamento == filtro.IdDepartamento);
        }

        var resultado = await consulta
            .Select(i => new CabeceraConsumoInterno
            {
                IdCabeceraConsumoInterno = i.id_cabecera_consumo_interno,
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
                NombreCreadoPor = i.nombre_creado_por
            })
            .Distinct()
            .ToListAsync();

        if (usuario != null)
        {
            switch (usuario.posicion_id)
            {
                case 1:  // Administrador
                    return resultado.ToList();
                case 2:  // Director
                    return resultado.Where(i => i.IdDepartamento == usuario.id_departamento).ToList();
                case 3:  // Gerente Area
                    return resultado.Where(i => i.IdSucursal == usuario.id_sucursal && i.IdDepartamento == usuario.id_departamento).ToList();
                case 4:  // Depachador
                    return resultado.Where(i => i.IdSucursal == usuario.id_sucursal).ToList();
                case 5:  // Solicitante
                    return resultado.Where(i => i.CreadoPor == usuario.correo && i.IdDepartamento == usuario.id_departamento && i.IdSucursal == usuario.id_sucursal).ToList();
            }
        }

        return resultado;
    }

    public async Task<List<CabeceraConsumoInterno>> ObtenerConsumosInternosSegunPosicionUsuario()
    {
        var allItems = await ObtenerConsumosInternos();
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        var usuario = await _context.UsuariosCI.FirstOrDefaultAsync(el => el.correo == identity.Name);

        if (usuario != null)
        {
            switch (usuario.posicion_id)
            {
                case 1:  // Administrador
                    return allItems.ToList();
                case 2:  // Director
                    return allItems.Where(i => i.IdDepartamento == usuario.id_departamento).ToList();
                case 3:  // Gerente Area
                    return allItems.Where(i => i.IdSucursal == usuario.id_sucursal && i.IdDepartamento == usuario.id_departamento).ToList();
                case 4:  // Depachador
                    return allItems.Where(i => i.IdSucursal == usuario.id_sucursal).ToList();
                case 5:  // Solicitante
                    return allItems.Where(i => i.CreadoPor == usuario.correo && i.IdDepartamento == usuario.id_departamento && i.IdSucursal == usuario.id_sucursal).ToList();
            }
        }
        return null;
    }

    public async Task<List<CabeceraConsumoInterno>> ObtenerConsumosInternos()
    {
        var cache = _memoryCache.Get<List<CabeceraConsumoInterno>>("ConsumosInternos");
        if (cache == null)
        {
            cache = await _context
                .CabeceraConsumosInternos.Select(i => new CabeceraConsumoInterno
                {
                    IdCabeceraConsumoInterno = i.id_cabecera_consumo_interno,
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
                    CantidadLineas = i.LineasConsumosInternos.Count,
                    NombreCreadoPor = i.nombre_creado_por
                })
                .ToListAsync();
            _memoryCache.Set<List<CabeceraConsumoInterno>>(
                "ConsumosInternos",
                cache,
                DateTimeOffset.Now.AddMinutes(5)
            );
        }
        return await Task.FromResult(cache.OrderBy(i => i.IdCabeceraConsumoInterno.Value).ToList());
    }

    public async Task<CabeceraConsumoInterno> ObtenerConsumoInternoPorId(int idSolicitud)
    {

        if (idSolicitud == 0 || idSolicitud  == null)
        {
            throw new InvalidDataException("El id de la solicitud no puede ser 0.");
        }

        var allItems = await ObtenerConsumosInternos();
        var item = allItems.Where(i => i.IdCabeceraConsumoInterno == idSolicitud).FirstOrDefault().Clone();
        if (item != null)
        {
            item.Lineas = _context.LineasConsumosInternos.Where(i => i.cabecera_consumo_interno_id == idSolicitud)
            .Select(i => new LineasConsumoInterno
            {
                IdLineaConsumoInterno = i.id_linea_consumo_interno,
                CabeceraConsumoInternoId = i.cabecera_consumo_interno_id,
                IdProducto = i.id_producto,
                NoProducto = i.no_producto,
                Descripcion = i.descripcion,
                PrecioUnitario = i.precio_unitario,
                Cantidad = i.cantidad,
                IdUnidadMedida = i.id_unidad_medida,
                CodigoUnidadMedida = i.codigo_unidad_medida,
                Total = i.total,
                CostoUnitario = i.costo_unitario
            })
            .OrderBy(i => i.IdLineaConsumoInterno)
            .ToList();
        }
        return item ?? throw new Exception("Consumo interno no encontrado.");
    }

    public async Task<int> ObtenerCantidadConsumoInternos()
    {

        var allItems = await ObtenerConsumosInternos();
        var identity = _httpContextAccessor.HttpContext!.User.Identity as ClaimsIdentity;
        var usuario = await _context.UsuariosCI.FirstOrDefaultAsync(el => el.correo == identity!.Name);
        var resultado = new List<CabeceraConsumoInterno>();

        switch (usuario.posicion_id)
        {
            case 1: // Administrador
                resultado = allItems.ToList();
                break;

            case 2: // Director
                resultado = allItems.Where(i => i.IdSucursal == usuario.id_sucursal && i.IdDepartamento == usuario.id_departamento).ToList();
                break;

            case 3: // Gerente Area
                resultado = allItems.Where(i => i.IdSucursal == usuario.id_sucursal && i.IdDepartamento == usuario.id_departamento).ToList();
                break;

            case 4: // Despachador
                resultado = allItems.Where(i => i.IdSucursal == usuario.id_sucursal).ToList();
                break;

            case 5: // Solicitante
                resultado = allItems.Where(i => i.CreadoPor == usuario.correo && i.IdDepartamento == usuario.id_departamento && i.IdSucursal == usuario.id_sucursal).ToList();
                break;
        }

        return resultado.ToList().Count();
    }

    public async Task<CabeceraConsumoInterno> ObtenerConsumoInterno(int id)
    {
        if (id == 0 || id == null)
        {
            throw new InvalidDataException("El id del consumo interno no puede ser 0.");
        }

        var allItems = await ObtenerConsumosInternos();
        var item = allItems.Where(i => i.IdCabeceraConsumoInterno == id).FirstOrDefault().Clone();
        if (item != null)
        {
            item.Lineas = _context
                .LineasConsumosInternos.Where(i => i.cabecera_consumo_interno_id == id)
                .Select(i => new LineasConsumoInterno
                {
                    IdLineaConsumoInterno = i.id_linea_consumo_interno,
                    CabeceraConsumoInternoId = i.cabecera_consumo_interno_id,
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
                    CostoUnitario = i.costo_unitario,
                })
                .OrderBy(i => i.IdLineaConsumoInterno)
                .ToList();
        }
        return item;
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("ConsumosInternos");
        await ObtenerConsumosInternos();
        return true;
    }

}