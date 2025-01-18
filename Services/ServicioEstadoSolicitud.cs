using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Security.Claims;
using Bellon.API.Liquidacion.Interfaces;
using Bellon.API.Liquidacion.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bellon.API.Liquidacion.Services;

public class ServicioEstadoSolicitud : IServicioEstadoSolicitud
{
    private readonly Liquidacion.DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;
    private readonly IServicioNumeroSerie _servicioNumeroSerie;
    private readonly IServicioHistLlegada _servicioHistLlegada;

    public ServicioEstadoSolicitud(
        IHttpContextAccessor httpContextAccessor,
        Liquidacion.DataBase.AppDataBase context,
        IOptions<AppSettings> settings,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IServicioAutorizacion servicioAutorizacion,
        IServicioHistLlegada servicioHistLlegada,
        IServicioNumeroSerie servicioNumeroSerie
    )
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _servicioAutorizacion = servicioAutorizacion;
        _servicioHistLlegada = servicioHistLlegada;
        _servicioNumeroSerie = servicioNumeroSerie;
    }

    public Task<List<EstadoSolicitud>> ObtenerEstadoSolicitudes()
    {
        var cache = _memoryCache.Get<List<EstadoSolicitud>>("EstadoSolicitudes");
        if (cache == null)
        {
            cache = _context
                .EstadosSolicitudes.Select(i => new EstadoSolicitud
                {
                    IdEstadoSolicitud = i.id_estado_solicitud,
                    Descripcion = i.descripcion,
                    Estado = i.estado
                })
                .ToList();
            _memoryCache.Set<List<EstadoSolicitud>>(
                "EstadoSolicitudes",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return Task.FromResult(cache.OrderBy(i => i.IdEstadoSolicitud.Value).ToList());
    }

     public async Task<List<EstadoSolicitud>> ObtenerEstadoSolicitud(int id)
    {
        var allItems = await ObtenerEstadoSolicitudes();
        return allItems.Where(i => i.IdEstadoSolicitud == id).ToList();
    }

    public async Task<List<EstadoSolicitud>> GuardarEstadoSolicitud(EstadoSolicitud item)
    {
        if (item.IdEstadoSolicitud.HasValue)
        {
            var oldItem = _context
                .EstadosSolicitudes.Where(i => i.id_estado_solicitud == item.IdEstadoSolicitud.Value)
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
            var newItemData = new DataBase.Models.EstadosSolicitudes
            {
                descripcion = item.Descripcion,
                estado = item.Estado
            };
            var newItem = _context.EstadosSolicitudes.Add(newItemData);
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

    public async Task<List<EstadoSolicitud>> EliminarEstadoSolicitud(int id)
    {
        var oldItem = _context.EstadosSolicitudes.Where(i => i.id_estado_solicitud == id).FirstOrDefault();
        if (oldItem != null)
        {
            _context.EstadosSolicitudes.Remove(oldItem);
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
        _memoryCache.Remove("Solicitudes");
        await ObtenerEstadoSolicitudes();
        return true;
    }
}

