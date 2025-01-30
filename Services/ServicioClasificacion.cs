using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Bellon.API.Liquidacion.Classes;
using Bellon.API.Liquidacion.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bellon.API.Liquidacion.Services;

public class ServicioClasificacion : IServicioClasificacion
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioClasificacion(
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
    }

    public async Task<List<LSCentralClasificacion>> ObtenerClasificacionesERP()
    {
        var cache = _memoryCache.Get<LSCentralClasificacionArray>("ClasificacionesERP");
        if (cache == null)
        {
            var token = await _servicioAutorizacion.ObtenerTokenBC();
            var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
            var httpResponseMessage = await httpClient.GetAsync(
                "GruposContProductoGeneral?$expand=GruposContIVAProducto"
            );
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                cache = await JsonSerializer.DeserializeAsync<LSCentralClasificacionArray>(
                    contentStream
                );
                if (cache != null)
                {
                    _memoryCache.Set<LSCentralClasificacionArray>(
                        "ClasificacionesERP",
                        cache,
                        DateTimeOffset.Now.AddMinutes(60)
                    );
                }
            }
            else
            {
                throw new Exception("Error en el servicio de Clasificaciones de LS Central");
            }
        }

        return cache
            .value.Where(i => !string.IsNullOrEmpty(i.Descripcion))
            .OrderBy(i => i.Descripcion)
            .ToList();
    }

    public async Task<LSCentralClasificacion> ObtenerClasificacionERP(string id)
    {
        var token = await _servicioAutorizacion.ObtenerTokenBC();
        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
        var httpResponseMessage = await httpClient.GetAsync(
            "GruposContProductoGeneral?$expand=GruposContIVAProducto"
        );
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var item = await JsonSerializer.DeserializeAsync<LSCentralClasificacion>(
                contentStream
            );
            return item;
        }
        else
        {
            throw new Exception("Error en el servicio de Clasificaciones de LS Central");
        }
    }

    public async Task<List<Classes.Clasificacion>> ObtenerClasificaciones()
    {
        var cache = _memoryCache.Get<List<Clasificacion>>("Clasificaciones");
        if (cache == null)
        {
            cache = _context
                .Clasificaciones.Select(i => new Clasificacion
                {
                    IdClasificacion = i.id_clasificacion,
                    IdGrupoContProductoGeneral = i.id_grupo_cont_producto_general,
                    CodigoClasificacion = i.codigo_clasificacion,
                    Descripcion = i.descripcion,
                    Estado = i.estado,
                })
                .ToList();
            _memoryCache.Set<List<Clasificacion>>(
                "Clasificaciones",
                cache,
                DateTimeOffset.Now.AddMinutes(5)
            );
        }
        return cache;
    }

    public async Task<Classes.Clasificacion> ObtenerClasificacion(int? id)
    {
        var allItems = await ObtenerClasificaciones();
        return allItems.Where(i => i.IdClasificacion == id).FirstOrDefault().Clone();
    }

    public async Task<Classes.Clasificacion> GuardarClasificacion(Classes.Clasificacion item)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (item.IdClasificacion.HasValue)
        {
            var oldItem = _context
                .Clasificaciones.Where(i =>
                    i.id_clasificacion == item.IdClasificacion.Value
                )
                .FirstOrDefault();
            if (oldItem != null)
            {

                oldItem.id_grupo_cont_producto_general = item.IdGrupoContProductoGeneral;
                oldItem.codigo_clasificacion = item.CodigoClasificacion;
                oldItem.descripcion = item.Descripcion;
                oldItem.estado = item.Estado;

                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al actualizar el registro: <" + ex.Message + ">");
                }

                await RefrescarCache();
                return await ObtenerClasificacion(oldItem.id_clasificacion);
            }
        }
        else
        {
            var newItem = new DataBase.Models.Clasificaciones
            {
                id_clasificacion = item.IdClasificacion,
                id_grupo_cont_producto_general = item.IdGrupoContProductoGeneral,
                codigo_clasificacion = item.CodigoClasificacion,
                descripcion = item.Descripcion,
                estado = item.Estado,
            };

            _context.Clasificaciones.Add(newItem);

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el registro: <" + ex.Message + ">");
            }

            await RefrescarCache();
            return await ObtenerClasificacion(newItem.id_clasificacion);
        }
        return null;
    }

    public async Task<Classes.Clasificacion> EliminarClasificacion(int id)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        var oldItem = _context.Clasificaciones.Where(i => i.id_clasificacion == id).FirstOrDefault();
        if (oldItem != null)
        {
            _context.Clasificaciones.Remove(oldItem);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el registro: <" + ex.Message + ">");
            }
            await RefrescarCache();
            return await ObtenerClasificacion(oldItem.id_clasificacion);
        }
        return null;
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("Clasificaciones");
        await ObtenerClasificacionesERP();
        await ObtenerClasificaciones();
        return true;
    }
}
