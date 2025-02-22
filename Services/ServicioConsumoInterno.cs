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

    public async Task<List<CabeceraConsumoInterno>> ObtenerConsumosInternosDelUsuarioSolicitantePorEstado(int? estadoSolicitudId)
    {
        var allItems = await ObtenerConsumosInternos();
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

    public Task<List<CabeceraConsumoInterno>> ObtenerConsumosInternos()
    {
        var cache = _memoryCache.Get<List<CabeceraConsumoInterno>>("ConsumosInternos");
        if (cache == null)
        {
            cache = _context
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
                .ToList();
            _memoryCache.Set<List<CabeceraConsumoInterno>>(
                "ConsumosInternos",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return Task.FromResult(cache.OrderBy(i => i.IdCabeceraConsumoInterno.Value).ToList());
    }

    public async Task<CabeceraConsumoInterno> ObtenerConsumoInternoPorId(int idSolicitud)
    {
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
                Total = i.total
            })
            .OrderBy(i => i.IdLineaConsumoInterno)
            .ToList();
        }
        return item ?? throw new Exception("Consumo interno no encontrado.");
    }

    public async Task<List<CabeceraConsumoInterno>> ObtenerConsumoInternoPorEstadoSolicitud(int? estadoSolicitudId)
    {
        var allItems = await ObtenerConsumosInternos();
        return allItems.Where(i => i.IdEstadoSolicitud == estadoSolicitudId).ToList();
    }

    public async Task<int> ObtenerCantidadConsumoInternos()
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        var allItems = await ObtenerConsumosInternos();
        return allItems.Where(i => i.CreadoPor == identity!.Name).ToList().Count;
    }

    public async Task<CabeceraConsumoInterno> ObtenerConsumoInterno(int id)
    {
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
                    Nota = i.nota ?? ""
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