using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using Bellon.API.ConsumoInterno.Classes;
using Bellon.API.ConsumoInterno.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bellon.API.ConsumoInterno.Services;

public class ServicioNumeroSerie : IServicioNumeroSerie
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioNumeroSerie(
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

    public async Task<string> ObtenerNumeroDocumento(int id)
    {
        var item = _context.NoSeries.Where(i => i.id_no_serie == id).FirstOrDefault();
        if (item != null)
        {
            int consecutivo =
                int.Parse(item.secuencia_inicial) > int.Parse(item.ultima_secuencia_utilizada)
                    ? int.Parse(item.secuencia_inicial)
                    : int.Parse(item.ultima_secuencia_utilizada) + 1;
            item.ultima_secuencia_utilizada = consecutivo.ToString();
            item.fecha_ultima_secuencia_utilizada = DateTime.UtcNow;
            _context.SaveChanges();

            return string.Format(
                "{0}{1}",
                item.codigo,
                consecutivo.ToString().PadLeft(_settings.CantidadDigitosDocumento, '0')
            );
        }
        else
        {
            throw new Exception("No se encontró NoSerie");
        }
    }
}
