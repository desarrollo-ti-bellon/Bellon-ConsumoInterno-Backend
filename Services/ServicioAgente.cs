using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using Bellon.API.Liquidacion.Classes;
using Bellon.API.Liquidacion.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bellon.API.Liquidacion.Services;

public class ServicioAgente : IServicioAgente
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioAgente(
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

    public async Task<List<Agente>> ObtenerAgentes()
    {
        var cache = _memoryCache.Get<List<Agente>>("Agentes");
        if (cache == null)
        {
            cache = _context
                .Agentes.Select(i => new Agente
                {
                    IdAgente = i.id_agente,
                    Nombre = i.nombre,
                    Estado = i.estado,
                    Tipo = i.tipo,
                })
                .ToList();
            _memoryCache.Set<List<Agente>>("Agentes", cache, DateTimeOffset.Now.AddMinutes(30));
        }
        return cache.OrderBy(i => i.IdAgente.Value).ToList();
    }

    public async Task<List<Agente>> ObtenerAgentesActivos()
    {
        var allItems = await ObtenerAgentes();
        return allItems.Where(i => i.Estado).ToList();
    }

    public async Task<Agente> ObtenerAgente(int id)
    {
        var allItems = await ObtenerAgentes();
        return allItems.Where(i => i.IdAgente == id).FirstOrDefault().Clone();
    }

    public async Task<Agente> GuardarAgente(Agente item)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (item.IdAgente.HasValue)
        {
            var oldItem = _context
                .Agentes.Where(i => i.id_agente == item.IdAgente.Value)
                .FirstOrDefault();
            if (oldItem != null)
            {
                oldItem.nombre = item.Nombre;
                oldItem.estado = item.Estado;
                oldItem.tipo = item.Tipo;
                oldItem.fecha_modificado = DateTime.UtcNow;
                oldItem.modificado_por = identity.Name;
                _context.SaveChanges();
                await RefrescarCache();
                return await ObtenerAgente(oldItem.id_agente);
            }
        }
        else
        {
            var newItem = _context.Agentes.Add(
                new DataBase.Models.Agentes
                {
                    nombre = item.Nombre,
                    estado = item.Estado,
                    tipo = item.Tipo,
                    fecha_creado = DateTime.UtcNow,
                    creado_por = identity.Name,
                }
            );
            _context.SaveChanges();
            await RefrescarCache();
            return await ObtenerAgente(newItem.Entity.id_agente);
        }
        return null;
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("Agentes");
        await ObtenerAgentes();
        return true;
    }
}
