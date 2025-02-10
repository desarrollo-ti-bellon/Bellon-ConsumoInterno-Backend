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
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Bellon.API.ConsumoInterno.Services;

public class ServicioNotas : IServicioNotas
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    private DateTime fechaLimite;

    public ServicioNotas(
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
        fechaLimite = DateTime.Now.AddDays(-3);
    }

    public async Task<List<Notas>> ObtenerNotas()
    {
        var cache = _memoryCache.Get<List<Notas>>("Notas");
        if (cache == null)
        {
            cache = _context
                .Notas.Select(i => new Notas
                {
                    IdNota = i.id_nota,
                    IdDocumento = i.id_documento,
                    TipoDocumento = i.tipo_documento,
                    NoDocumento = i.no_documento,
                    UsuarioDestino = i.usuario_destino ?? "",
                    Descripcion = i.descripcion,
                    FechaCreado = i.fecha_creado,
                    CreadoPor = i.creado_por,
                    FechaModificado = i.fecha_modificado,
                    ModificadoPor = i.modificado_por,
                })
                .ToList();
            _memoryCache.Set<List<Notas>>(
                "Notas",
                cache,
                DateTimeOffset.Now.AddMinutes(5)
            );
        }
        return cache;
    }

    public async Task<Notas> ObtenerNota(int id)
    {
        var allItems = await ObtenerNotas();
        return allItems.Where(i => i.IdNota == id).FirstOrDefault().Clone() ?? new Notas();
    }

    public async Task<List<Notas>> ObtenerNotasPorParametros(string? UsuarioDestino, string? TipoDocumento)
    {
        if (string.IsNullOrEmpty(UsuarioDestino) && string.IsNullOrEmpty(TipoDocumento))
        {
            throw new Exception("Debe especificar uno de los tres parámetros ( CreadoPor | UsuarioDestino | TipoDocumento )");
        }

        var allItems = await ObtenerNotas();
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        return allItems
            .Where(i => (
                ((string.IsNullOrEmpty(identity?.Name) || i.CreadoPor == identity.Name) ||
                (string.IsNullOrEmpty(UsuarioDestino) || i.UsuarioDestino == UsuarioDestino || string.IsNullOrEmpty(i.UsuarioDestino))) &&
                (string.IsNullOrEmpty(TipoDocumento) || i.TipoDocumento == TipoDocumento) &&
                (i.FechaCreado >= fechaLimite)
            ))
            .OrderByDescending(i => i.IdNota)
            .ToList();
    }

    public async Task<List<Notas>> ObtenerNotasDelUsuario(string UsuarioDestino)
    {
        if (string.IsNullOrEmpty(UsuarioDestino))
        {
            throw new Exception(
                "Debe especificar uno de los dos parámetros UsuarioDestino"
            );
        }
        var allItems = await ObtenerNotas();
        return allItems.Where(i => i.UsuarioDestino == UsuarioDestino).ToList();
    }

    public async Task<int> ObtenerCantidadDeNotas(string UsuarioDestino)
    {
        var allItems = await ObtenerNotas();
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        return allItems
            .Where(i => (
                ((string.IsNullOrEmpty(identity?.Name) || i.CreadoPor == identity.Name) ||
                (string.IsNullOrEmpty(UsuarioDestino) || i.UsuarioDestino == UsuarioDestino)) &&
                (i.FechaCreado >= fechaLimite)
            ))
            .OrderByDescending(i => i.IdNota)
            .ToList().Count();
    }

    public async Task<List<Notas>> GuardarNotas(Notas item)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (item.IdNota != null)
        {
            var oldItem = _context.Notas.Where(i => i.id_nota == item.IdNota).FirstOrDefault();
            if (oldItem != null)
            {
                oldItem.id_documento = item.IdDocumento;
                oldItem.tipo_documento = item.TipoDocumento;
                oldItem.no_documento = item.NoDocumento;
                oldItem.usuario_destino = item.UsuarioDestino;
                oldItem.descripcion = item.Descripcion;
                oldItem.fecha_modificado = DateTime.Now;
                oldItem.modificado_por = identity.Name;
                _context.SaveChanges();
                await RefrescarCache();
                return await ObtenerNotasPorParametros(identity.Name, item.TipoDocumento);
            }
        }
        else
        {
            var newItem = _context.Notas.Add(
                new DataBase.Notas
                {
                    id_documento = item.IdDocumento,
                    tipo_documento = item.TipoDocumento,
                    no_documento = item.NoDocumento,
                    usuario_destino = item.UsuarioDestino,
                    descripcion = item.Descripcion,
                    fecha_creado = DateTime.Now,
                    creado_por = identity.Name,
                }
            );
            _context.SaveChanges();
            await RefrescarCache();
            return await ObtenerNotasPorParametros(identity.Name, newItem.Entity.tipo_documento);
        }
        return null;
    }

    public async Task<List<Classes.Notas>> EliminarNotas(int id)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        var oldItem = _context.Notas.Where(i => i.id_nota == id).FirstOrDefault();
        if (oldItem != null)
        {
            _context.Notas.Remove(oldItem);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el registro: <" + ex.Message + ">");
            }
            await RefrescarCache();
            return await ObtenerNotasPorParametros(identity.Name, oldItem.tipo_documento);
        }
        return null;
    }

    public async Task<bool> RefrescarCache(bool incluyeProduccion = false)
    {
        _memoryCache.Remove("Notas");
        await ObtenerNotas();
        return true;
    }

}
