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

public class ServicioTipoContenedor : IServicioTipoContenedor
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioTipoContenedor(
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

    public async Task<List<TipoContenedor>> ObtenerTipoContenedores()
    {
        var cache = _memoryCache.Get<List<TipoContenedor>>("TipoContenedores");
        if (cache == null)
        {
            cache = _context
                .TipoContenedores.Select(i => new TipoContenedor
                {
                    IdTipoContenedor = i.id_tipo_contenedor,
                    Descripcion = i.descripcion,
                    Estado = i.estado,
                })
                .ToList();
            _memoryCache.Set<List<TipoContenedor>>(
                "TipoContenedores",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return cache.OrderBy(i => i.IdTipoContenedor.Value).ToList();
        ;
    }

    public async Task<List<TipoContenedor>> ObtenerTipoContenedoresActivos()
    {
        var allItems = await ObtenerTipoContenedores();
        return allItems.Where(i => i.Estado).ToList();
    }

    public async Task<TipoContenedor> ObtenerTipoContenedor(int id)
    {
        var allItems = await ObtenerTipoContenedores();
        return allItems.Where(i => i.IdTipoContenedor == id).FirstOrDefault().Clone();
    }

    public async Task<TipoContenedor> GuardarTipoContenedor(TipoContenedor item)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (item.IdTipoContenedor.HasValue)
        {
            var oldItem = _context
                .TipoContenedores.Where(i => i.id_tipo_contenedor == item.IdTipoContenedor.Value)
                .FirstOrDefault();
            if (oldItem != null)
            {
                oldItem.descripcion = item.Descripcion;
                oldItem.estado = item.Estado;
                oldItem.fecha_modificado = DateTime.UtcNow;
                oldItem.modificado_por = identity.Name;
                _context.SaveChanges();
                _memoryCache.Remove("TipoContenedores");
                await ObtenerTipoContenedores();
                return await ObtenerTipoContenedor(oldItem.id_tipo_contenedor);
            }
        }
        else
        {
            var newItem = _context.TipoContenedores.Add(
                new DataBase.Models.TipoContenedores
                {
                    descripcion = item.Descripcion,
                    estado = item.Estado,
                    fecha_creado = DateTime.UtcNow,
                    creado_por = identity.Name,
                }
            );
            _context.SaveChanges();
            _memoryCache.Remove("TipoContenedores");
            await ObtenerTipoContenedores();
            return await ObtenerTipoContenedor(newItem.Entity.id_tipo_contenedor);
        }
        return null;
    }
}
