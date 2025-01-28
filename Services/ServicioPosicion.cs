using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using Bellon.API.Liquidacion.Classes;
using Bellon.API.Liquidacion.DataBase.Models;
using Bellon.API.Liquidacion.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bellon.API.Liquidacion.Services;

public class ServicioPosicion : IServicioPosicion
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioPosicion(
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
    public async Task<List<Posicion>> ObtenerPosiciones()
    {
        var cache = _memoryCache.Get<List<Posicion>>("Posicion");
        if (cache == null)
        {
            cache = _context
                .Posiciones.Select(i => new Posicion
                {
                    PosicionId = i.posicion_id,
                    Descripcion = i.descripcion,
                    LimiteMaximoPermitido = i.limite_maximo_permitido,
                })
                .ToList();
            _memoryCache.Set<List<Posicion>>(
                "Posicion",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return cache.OrderBy(i => i.PosicionId.Value).ToList();
    }

    public async Task<Posicion> ObtenerPosicion(int? id)
    {
        var allItems = await ObtenerPosiciones();
        return allItems.Where(i => i.PosicionId == id).FirstOrDefault().Clone() ?? null;
    }

}
