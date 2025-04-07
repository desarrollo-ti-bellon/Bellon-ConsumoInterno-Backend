using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using Bellon.API.ConsumoInterno.Classes;
using Bellon.API.ConsumoInterno.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bellon.API.ConsumoInterno.Services;

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
    public async Task<List<PosicionUsuarioCI>> ObtenerPosiciones()
    {
        var cache = _memoryCache.Get<List<PosicionUsuarioCI>>("PosicionesCI");
        if (cache == null)
        {
            cache = _context
            .PosicionesUsuariosCI.Select(i => new PosicionUsuarioCI
            {
                PosicionId = i.posicion_id,
                Descripcion = i.descripcion,
                CrearSolicitud = i.crear_solicitud,
                EnviarSolicitud = i.enviar_solicitud,
                AprobarSolicitud = i.aprobar_solicitud,
                RechazarSolicitud = i.rechazar_solicitud,
                ConfirmarSolicitud = i.confirmar_solicitud,
                EntregarSolicitud = i.entregar_solicitud,
                CancelarSolicitud = i.cancelar_solicitud
            })
            .ToList();
            _memoryCache.Set<List<PosicionUsuarioCI>>(
                "PosicionesCI",
                cache,
                DateTimeOffset.Now.AddMinutes(30)
            );
        }
        return cache.OrderBy(i => i.PosicionId.Value).ToList();
    }

    public async Task<PosicionUsuarioCI> ObtenerPosicion(int? id)
    {

        if (id.HasValue == false || id == 0)
        {
            throw new InvalidDataException("El id de la posición no puede ser nulo o vacío");
        }

        var allItems = await ObtenerPosiciones();
        return allItems.Where(i => i.PosicionId == id).FirstOrDefault().Clone() ?? null;
    }

}
