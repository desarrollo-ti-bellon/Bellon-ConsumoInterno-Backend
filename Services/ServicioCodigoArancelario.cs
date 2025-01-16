using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using Bellon.API.Liquidacion.Classes;
using Bellon.API.Liquidacion.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bellon.API.Liquidacion.Services;

public class ServicioCodigoArancelario : IServicioCodigoArancelario
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioCodigoArancelario(
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

    public async Task<List<CodigoArancelario>> ObtenerCodigoArancelarios()
    {
        var cache = _memoryCache.Get<List<CodigoArancelario>>("CodigoArancelarios");
        if (cache == null)
        {
            cache = _context
                .CodigoArancelarios.Select(i => new CodigoArancelario
                {
                    CodigoPais = i.codigo_pais,
                    Descripcion = i.descripcion_cod_arancelario,
                    IdCodArancelario = i.id_cod_arancelario,
                    IdCodigoArancelario = i.id_codigo_arancelario,
                    IdPais = i.id_pais,
                    NoCodArancelario = i.no_cod_arancelario,
                    PorcientoGravamen = i.porciento_gravamen,
                    PorcientoSelectivo = i.porciento_selectivo,
                })
                .ToList();
            _memoryCache.Set<List<CodigoArancelario>>(
                "CodigoArancelarios",
                cache,
                DateTimeOffset.Now.AddMinutes(5)
            );
        }
        return cache;
    }

    public async Task<CodigoArancelario> ObtenerCodigoArancelario(int id)
    {
        var allItems = await ObtenerCodigoArancelarios();
        return allItems.Where(i => i.IdCodigoArancelario == id).FirstOrDefault().Clone();
    }

    public async Task<CodigoArancelario> ObtenerPorcentajesCodigoArancelario(string id)
    {
        var token = await _servicioAutorizacion.ObtenerTokenBC();
        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
        var httpResponseMessage = await httpClient.GetAsync("CodArancelarios(" + id + ")");
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<Classes.LSCentralCodigoArancelario>(
                contentStream
            );
            if (result != null)
            {
                var allItems = await ObtenerCodigoArancelarios();
                var item = allItems
                    .Where(i => i.IdCodArancelario.Equals(id))
                    .FirstOrDefault()
                    .Clone();
                if (item != null)
                {
                    return new CodigoArancelario
                    {
                        CodigoPais = item.CodigoPais,
                        IdCodigoArancelario = item.IdCodigoArancelario,
                        IdPais = item.IdPais,
                        NoCodArancelario = result.No,
                        IdCodArancelario = result.IdCodArancelario.ToString(),
                        PorcientoGravamen = item.PorcientoGravamen,
                        PorcientoSelectivo = item.PorcientoSelectivo,
                        Descripcion = result.Descripcion,
                    };
                }
            }
            return null;
        }
        return null;
    }

    public async Task<CodigoArancelario> GuardarCodigoArancelario(CodigoArancelario item)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (item.IdCodigoArancelario.HasValue)
        {
            var oldItem = _context
                .CodigoArancelarios.Where(i =>
                    i.id_codigo_arancelario == item.IdCodigoArancelario.Value
                )
                .FirstOrDefault();
            if (oldItem != null)
            {
                oldItem.codigo_pais = item.CodigoPais;
                oldItem.descripcion_cod_arancelario = item.Descripcion;
                oldItem.id_cod_arancelario = item.IdCodArancelario;
                oldItem.id_pais = item.IdPais;
                oldItem.no_cod_arancelario = item.NoCodArancelario;
                oldItem.porciento_gravamen = item.PorcientoGravamen;
                oldItem.porciento_selectivo = item.PorcientoSelectivo;
                oldItem.fecha_modificado = DateTime.UtcNow;
                oldItem.modificado_por = identity.Name;
                _context.SaveChanges();
                await RefrescarCache();
                return await ObtenerCodigoArancelario(oldItem.id_codigo_arancelario);
            }
        }
        else
        {
            var newItem = _context.CodigoArancelarios.Add(
                new DataBase.Models.CodigoArancelarios
                {
                    codigo_pais = item.CodigoPais,
                    descripcion_cod_arancelario = item.Descripcion,
                    id_cod_arancelario = item.IdCodArancelario,
                    id_pais = item.IdPais,
                    no_cod_arancelario = item.NoCodArancelario,
                    porciento_gravamen = item.PorcientoGravamen,
                    porciento_selectivo = item.PorcientoSelectivo,
                    fecha_creado = DateTime.UtcNow,
                    creado_por = identity.Name,
                }
            );
            _context.SaveChanges();
            await RefrescarCache();
            return await ObtenerCodigoArancelario(newItem.Entity.id_codigo_arancelario);
        }
        return null;
    }

    public async Task<List<LSCentralCodigoArancelario>> ObtenerCodigosArancelarioERP()
    {
        var cache = _memoryCache.Get<LSCentralCodigoArancelarioArray>("CodigoArancelariosERP");
        if (cache == null)
        {
            var token = await _servicioAutorizacion.ObtenerTokenBC();
            var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
            var httpResponseMessage = await httpClient.GetAsync("CodArancelarios");
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                cache =
                    await JsonSerializer.DeserializeAsync<Classes.LSCentralCodigoArancelarioArray>(
                        contentStream
                    );
                if (cache != null)
                {
                    _memoryCache.Set<LSCentralCodigoArancelarioArray>(
                        "CodigoArancelariosERP",
                        cache,
                        DateTimeOffset.Now.AddMinutes(60)
                    );
                }
            }
            else
            {
                throw new Exception("Error en el servicio de CodArancelarios de LS Central");
            }
        }
        return cache
            .value.Where(i => !string.IsNullOrEmpty(i.Descripcion))
            .OrderBy(i => i.Descripcion)
            .ToList();
    }

    public async Task<LSCentralCodigoArancelario> ObtenerCodigoArancelarioERP(string id)
    {
        var token = await _servicioAutorizacion.ObtenerTokenBC();
        var httpClient = _httpClientFactory.CreateClient("LSCentral-APIs");
        httpClient = _httpClientFactory.CreateClient("LSCentral-APIs");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token!.AccessToken);
        var httpResponseMessage = await httpClient.GetAsync("CodArancelarios(" + id + ")");
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var item = await JsonSerializer.DeserializeAsync<Classes.LSCentralCodigoArancelario>(
                contentStream
            );
            return item;
        }
        else
        {
            throw new Exception("Error en el servicio de CodArancelarios de LS Central");
        }
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("CodigoArancelarios");
        _memoryCache.Remove("CodigoArancelariosERP");
        await ObtenerCodigoArancelarios();
        await ObtenerCodigosArancelarioERP();
        return true;
    }
}
