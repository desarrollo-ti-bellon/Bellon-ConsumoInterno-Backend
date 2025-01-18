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

public class ServicioSolicitud : IServicioSolicitud
{
    private readonly Liquidacion.DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;
    private readonly IServicioNumeroSerie _servicioNumeroSerie;
    private readonly IServicioHistLlegada _servicioHistLlegada;

    public ServicioSolicitud(
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

    public Task<List<CabeceraSolicitud>> ObtenerSolicitudes()
    {
        var cache = _memoryCache.Get<List<CabeceraSolicitud>>("Solicitudes");
        if (cache == null)
        {
            cache = _context
                .CabeceraSolicitudes.Select(i => new CabeceraSolicitud
                {
                  
                })
                .ToList();
            _memoryCache.Set<List<CabeceraSolicitud>>(
                "Solicitudes",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return Task.FromResult(cache.OrderBy(i => i.IdSolicitud.Value).ToList());
    }

    public async Task<List<CabeceraSolicitud>> ObtenerSolicitudesPorId(int idSolicitud)
    {
        var allItems = await ObtenerSolicitudes();
        return allItems.Where(i => i.IdSolicitud == idSolicitud).ToList();
    }

    public async Task<List<CabeceraSolicitud>> ObtenerSolicitudesPorEstadoSolicitud(int estadoSolicitudId)
    {
        var allItems = await ObtenerSolicitudes();
        return allItems.Where(i => i.EstadoSolicitudId == estadoSolicitudId).ToList();
    }

    public async Task<int> ObtenerCantidadSolicitudesPorEstadoSolicitud(int estadoSolicitudId)
    {
        var allItems = await ObtenerSolicitudes();
        return allItems.Where(i => i.EstadoSolicitudId == estadoSolicitudId).ToList().Count;
    }

    public async Task<CabeceraSolicitud> ObtenerSolicitud(int id)
    {
        var allItems = await ObtenerSolicitudes();
        var item = allItems.Where(i => i.IdSolicitud == id).FirstOrDefault().Clone();
        if (item != null)
        {
            item.Lineas = _context
                .LineasSolicitudes.Where(i => i.cabecera_solicitud_id == id)
                .Select(i => new LineasSolicitud
                {
                   
                })
                .OrderBy(i => i.IdLineaSolicitud)
                .ToList();
        }
        return item;
    }

    public async Task<List<CabeceraSolicitud>> GuardarSolicitud(CabeceraSolicitud item)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (item.IdSolicitud.HasValue)
        {
            var oldItem = _context
                .CabeceraSolicitudes.Where(i => i.id_cabecera_solicitud == item.IdSolicitud.Value)
                .FirstOrDefault();
            if (oldItem != null)
            {
                //ACTUALIZA EL OBJETO EXISTENTE
                oldItem.comentario = item.Comentario;
                oldItem.usuario_asistente_control = item.UsuarioAsistenteControl;
                oldItem.usuario_asistente_contabilidad = item.UsuarioAsistenteContabilidad;
                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al actualizar el registro: <" + ex.Message + ">");
                }
                oldItem.total = item.Total;
                oldItem.fecha_modificado = DateTime.UtcNow;
                oldItem.modificado_por = identity!.Name;
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
                return await ObtenerSolicitudes();
            }
        }
        else
        {
            //SE INSERTA EL NUEVO ITEM
            var newItemData = new DataBase.Models.CabeceraSolicitudes
            {
               
                fecha_creado = item.FechaCreado,
                comentario = item.Comentario,
                creado_por = item.CreadoPor,
                modificado_por = item.ModificadoPor,
                fecha_modificado = item.FechaModificado,
                total = item.Total,
                id_departamento = item.IdDepartamento,
                usuario_asistente_control = item.UsuarioAsistenteControl,
                usuario_asistente_contabilidad = item.UsuarioAsistenteContabilidad,
            };
            var newItem = _context.CabeceraSolicitudes.Add(newItemData);
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
            return await ObtenerSolicitudes();
        }
        return null;
    }

    public async Task<List<CabeceraSolicitud>> GuardarLineasSolicitud(List<LineasSolicitud> items)
    {
        if (items.Count > 0)
        {
            var parent = _context
                .LineasSolicitudes.Where(i => i.id_linea_solicitud == items[0].IdLineaSolicitud)
                .FirstOrDefault();
            if (parent != null)
            {
                var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                foreach (var item in items)
                {
                    if (item.IdLineaSolicitud.HasValue)
                    {
                        var oldItem = _context
                            .LineasSolicitudes.Where(i =>
                                i.id_linea_solicitud == item.IdLineaSolicitud.Value
                                && i.cabecera_solicitud_id == item.CabeceraSolicitudId
                            )
                            .FirstOrDefault();
                        if (oldItem != null)
                        {
                            //ACTUALIZA EL OBJETO EXISTENTE
                          
                        }
                        else
                        {
                            throw new Exception(
                                "Error al actualizar la linea" + oldItem.id_linea_solicitud
                            );
                        }
                    }
                    else
                    {
                        //SE INSERTA EL NUEVO ITEM
                        var newItemData = new DataBase.Models.LineasSolicitudes
                        {
                           
                        };
                        var newItem = _context.LineasSolicitudes.Add(newItemData);
                        try
                        {
                            _context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(
                                "Error al crear el registro: <" + ex.Message + ">"
                            );
                        }
                    }
                }
            }
            //SE ACTUALIZA EL TOTAL DE LA CABECERA
            CalcularTotalCabecera(items[0].CabeceraSolicitudId);

            //SE LIMPIA LA CACHE Y SE VUELVE A POBLAR
            await RefrescarCache();

            //SE RETORNA EL OBJETO CREADO
            return await ObtenerSolicitudes();
        }
        else
        {
            throw new Exception(
                string.Format("No se encontró la solicitud {0}", items[0].CabeceraSolicitudId)
            );
        }
    }

    public void CalcularTotalCabecera(int id)
    {
        var item = _context
            .CabeceraSolicitudes.Where(i => i.id_cabecera_solicitud == id)
            .FirstOrDefault();
        if (item != null)
        {
            item.total = _context
                .LineasSolicitudes.Where(i => i.id_linea_solicitud == id)
                .Sum(i => i.precio_unitario * i.cantidad);
            _context.SaveChanges();
        }
    }

    public Task<List<CabeceraSolicitud>> RecuperarSolicitud(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<CabeceraSolicitud>> EliminarSolicitud(int id)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        var oldItem = _context.CabeceraSolicitudes.Where(i => i.id_cabecera_solicitud == id).FirstOrDefault();
        if (oldItem != null)
        {
            _context.CabeceraSolicitudes.Remove(oldItem);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el registro: <" + ex.Message + ">");
            }
            await RefrescarCache();
            return await ObtenerSolicitudes();
        }
        return null;
    }

    public async Task<List<CabeceraSolicitud>> EliminarLineaSolicitud(int id)
    {
        var oldItem = _context.LineasSolicitudes.Where(i => i.id_linea_solicitud == id).FirstOrDefault();
        if (oldItem!= null)
        {
            _context.LineasSolicitudes.Remove(oldItem);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el registro: <" + ex.Message + ">");
            }
            await RefrescarCache();
            return await ObtenerSolicitudesPorId(oldItem.cabecera_solicitud_id);
        }
        return null;
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("Solicitudes");
        await ObtenerSolicitudes();
        return true;
    }
}

