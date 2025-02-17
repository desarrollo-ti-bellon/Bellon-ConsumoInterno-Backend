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

namespace Bellon.API.ConsumoInterno.Services;

public class ServicioEstadoSolicitud : IServicioEstadoSolicitud
{
    private readonly ConsumoInterno.DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;
    private readonly IServicioNumeroSerie _servicioNumeroSerie;

    public ServicioEstadoSolicitud(
        IHttpContextAccessor httpContextAccessor,
        ConsumoInterno.DataBase.AppDataBase context,
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

    public Task<List<EstadoSolicitudCI>> ObtenerEstadoSolicitudes()
    {
        var cache = _memoryCache.Get<List<EstadoSolicitudCI>>("EstadoSolicitudes");
        if (cache == null)
        {
            cache = _context
                .EstadosSolicitudesCI.Select(i => new EstadoSolicitudCI
                {
                    IdEstadoSolicitud = i.id_estado_solicitud,
                    Descripcion = i.descripcion,
                    Estado = i.estado
                })
                .ToList();
            _memoryCache.Set<List<EstadoSolicitudCI>>(
                "EstadoSolicitudes",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return Task.FromResult(cache.OrderBy(i => i.IdEstadoSolicitud.Value).ToList());
    }

     public async Task<List<EstadoSolicitudCI>> ObtenerEstadoSolicitud(int id)
    {
        var allItems = await ObtenerEstadoSolicitudes();
        return allItems.Where(i => i.IdEstadoSolicitud == id).ToList();
    }

    public async Task<List<EstadoSolicitudCI>> GuardarEstadoSolicitud(EstadoSolicitudCI item)
    {
        if (item.IdEstadoSolicitud.HasValue)
        {
            var oldItem = _context
                .EstadosSolicitudesCI.Where(i => i.id_estado_solicitud == item.IdEstadoSolicitud.Value)
                .FirstOrDefault();
            if (oldItem != null)
            {
                //ACTUALIZA EL OBJETO EXISTENTE
                // oldItem.id_estado_solicitud = item.IdEstadoSolicitud;
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

                //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
                await RefrescarCache();

                //SE RETORNA EL OBJETO MODIFICADO
                return await ObtenerEstadoSolicitudes();
            }
        }
        else
        {
            //SE INSERTA EL NUEVO ITEM
            var newItemData = new DataBase.EstadosSolicitudesCI
            {
                descripcion = item.Descripcion,
                estado = item.Estado
            };
            var newItem = _context.EstadosSolicitudesCI.Add(newItemData);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear el registro: <" + ex.Message + ">");
            }

            //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
            await RefrescarCache();

            //SE RETORNA EL OBJETO CREADO
            return await ObtenerEstadoSolicitudes();
        }
        return null;
    }

    public async Task<List<EstadoSolicitudCI>> EliminarEstadoSolicitud(int id)
    {
        var oldItem = _context.EstadosSolicitudesCI.Where(i => i.id_estado_solicitud == id).FirstOrDefault();
        if (oldItem != null)
        {
            _context.EstadosSolicitudesCI.Remove(oldItem);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el registro: <" + ex.Message + ">");
            }
            await RefrescarCache();
            return await ObtenerEstadoSolicitudes();
        }
        return null;
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("EstadoSolicitudes");
        await ObtenerEstadoSolicitudes();
        return true;
    }
}

