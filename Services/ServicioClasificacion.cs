using System.Data.Common;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Bellon.API.ConsumoInterno.Classes;
using Bellon.API.ConsumoInterno.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bellon.API.ConsumoInterno.Services;

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
            var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
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

        if (String.IsNullOrEmpty(id))
        {
            throw new InvalidDataException("El id de la clasificación no puede ser nulo o vacío");
        }

        var token = await _servicioAutorizacion.ObtenerTokenBC();
        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs-Comunes");
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

    public async Task<List<Classes.ClasificacionCI>> ObtenerClasificaciones()
    {
        var cache = _memoryCache.Get<List<ClasificacionCI>>("ClasificacionesCI");
        if (cache == null)
        {
            cache = _context
                .ClasificacionesCI.Select(i => new ClasificacionCI
                {
                    IdClasificacion = i.id_clasificacion,
                    IdGrupoContProductoGeneral = i.id_grupo_cont_producto_general,
                    CodigoClasificacion = i.codigo_clasificacion,
                    Descripcion = i.descripcion,
                    Estado = i.estado,
                })
                .ToList();
            _memoryCache.Set<List<ClasificacionCI>>(
                "ClasificacionesCI",
                cache,
                DateTimeOffset.Now.AddMinutes(5)
            );
        }
        return cache;
    }

    public async Task<List<Classes.ClasificacionCI>> ObtenerClasificacionesActivas()
    {
        var cache = _memoryCache.Get<List<ClasificacionCI>>("ClasificacionesCIActivas");
        if (cache == null)
        {
            cache = _context
                .ClasificacionesCI.Select(i => new ClasificacionCI
                {
                    IdClasificacion = i.id_clasificacion,
                    IdGrupoContProductoGeneral = i.id_grupo_cont_producto_general,
                    CodigoClasificacion = i.codigo_clasificacion,
                    Descripcion = i.descripcion,
                    Estado = i.estado,
                })
                .Where(i => i.Estado == true)
                .ToList();
            _memoryCache.Set<List<ClasificacionCI>>(
                "ClasificacionesCIActivas",
                cache,
                DateTimeOffset.Now.AddMinutes(5)
            );
        }
        return cache;
    }

    public async Task<Classes.ClasificacionCI> ObtenerClasificacion(int? id)
    {

        if (id.HasValue == false)
        {
            throw new InvalidDataException("El id de la clasificación no puede ser nulo o vacío");
        }

        var allItems = await ObtenerClasificaciones();
        return allItems.Where(i => i.IdClasificacion == id).FirstOrDefault().Clone();
    }

    public async Task<Classes.ClasificacionCI> GuardarClasificacion(Classes.ClasificacionCI item)
    {

        if (item == null)
        {
            throw new InvalidDataException("los datos enviados no pueden ser nulos o vacios");
        }

        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (item.IdClasificacion.HasValue)
        {
            var oldItem = _context.
                ClasificacionesCI.Where(i =>
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
                catch (DbException ex)
                {
                    throw new Exception($"Error en la base de datos: < {ex.Message} >");
                }
                catch (Exception ex)
                {
                    var mensaje = ex.InnerException?.Message ?? ex.Message;
                    throw new InvalidDataException("Error al actualizar el registro: <" + mensaje + ">");
                }

                await RefrescarCache();
                return await ObtenerClasificacion(oldItem.id_clasificacion);
            }
        }
        else
        {
            var newItem = new DataBase.ClasificacionesCI
            {
                id_grupo_cont_producto_general = item.IdGrupoContProductoGeneral,
                codigo_clasificacion = item.CodigoClasificacion,
                descripcion = item.Descripcion,
                estado = item.Estado,
            };

            _context.ClasificacionesCI.Add(newItem);

            try
            {
                _context.SaveChanges();
            }
            catch (DbException ex)
            {
                throw new Exception($"Error en la base de datos: < {ex.Message} >");
            }
            catch (Exception ex)
            {
                var mensaje = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidDataException("Error al actualizar el registro: <" + mensaje + ">");
            }

            await RefrescarCache();
            return await ObtenerClasificacion(newItem.id_clasificacion);
        }
        return null;
    }

    public async Task<Classes.ClasificacionCI> EliminarClasificacion(int id)
    {

        if (id == null || id == 0)
        {
            throw new InvalidDataException("El id de la clasificación no puede ser nulo o vacío");
        }

        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        var oldItem = _context.ClasificacionesCI.Where(i => i.id_clasificacion == id).FirstOrDefault();
        if (oldItem != null)
        {
            _context.ClasificacionesCI.Remove(oldItem);

            try
            {
                _context.SaveChanges();
            }
            catch (DbException ex)
            {
                throw new Exception($"Error en la base de datos: < {ex.Message} >");
            }
            catch (Exception ex)
            {
                var mensaje = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidDataException("Error al eliminar el registro: <" + mensaje + ">");
            }

            await RefrescarCache();
            return await ObtenerClasificacion(oldItem.id_clasificacion);
        }
        return null;
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("ClasificacionesCI");
        _memoryCache.Remove("ClasificacionesERP");
        _memoryCache.Remove("ClasificacionesCIActivas");
        await ObtenerClasificacionesERP();
        await ObtenerClasificaciones();
        await ObtenerClasificacionesActivas();
        return true;
    }

}
