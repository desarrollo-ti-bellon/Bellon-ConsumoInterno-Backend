using System.Text.Json;
using Bellon.API.ConsumoInterno.Classes;
using Bellon.API.ConsumoInterno.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bellon.API.ConsumoInterno.Services;

public class ServicioAutorizacion : IServicioAutorizacion
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;

    public ServicioAutorizacion(
        DataBase.AppDataBase context,
        IOptions<AppSettings> settings,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache
    )
    {
        _context = context;
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
    }

    public async Task<bool> ValidarUsuarioPerfilAdminUsuario(string usuario)
    {

        if (string.IsNullOrEmpty(usuario))
        {
            throw new InvalidDataException(nameof(usuario));
        }

        var perfil = await _context.Perfil
        .Where(i => i.aplicacionId == _settings.AplicacionUsuarioId && i.nombre == "Administrador")
        .FirstOrDefaultAsync() ?? throw new InvalidDataException("Perfil no encontrado");
        var data = await _context
                .UsuarioPerfil.Where(i => i.perfilId == perfil.id && i.usuario.ToLower().Equals(usuario.ToLower())).FirstOrDefaultAsync();

        return data != null;
    }

    public Task<bool> ValidarUsuarioPerfil(string userName)
    {
        var cache = _memoryCache.Get<List<DataBase.Models.UsuarioPerfil>>("UsuarioPerfil");
        if (cache == null)
        {
            cache = _context
                .UsuarioPerfil.Include(i => i.perfil)
                .Where(i => i.perfil.aplicacionId == _settings.AplicacionId)
                .ToList();

            //SAVE DATA IN CACHE FOR 30 DAYS (OR CHANGES)
            _memoryCache.Set<List<DataBase.Models.UsuarioPerfil>>(
                "UsuarioPerfil",
                cache,
                DateTimeOffset.Now.AddMinutes(43200)
            );
        }
        var data = cache
            .Where(i => i.usuario.ToLower().Equals(userName.ToLower()))
            .FirstOrDefault();
        return (data == null) ? Task.FromResult(false) : Task.FromResult(true);
    }

    public async Task<Classes.BusinessCentralToken> ObtenerTokenBC()
    {
        var token = _memoryCache.Get<Classes.BusinessCentralToken>("Token-BusinessCentral");
        if (token == null)
        {
            var httpClient = _httpClientFactory.CreateClient("LSCentral-Token");
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>(
                    "client_secret",
                    _settings.LSCentralTokenClientSecret
                ),
                new KeyValuePair<string, string>("client_id", _settings.LSCentralTokenClientId),
                new KeyValuePair<string, string>(
                    "scope",
                    "https://api.businesscentral.dynamics.com/.default"
                ),
            };
            var formUrlEncodedContent = new FormUrlEncodedContent(parameters);
            var httpResponseMessage = await httpClient.PostAsync("", formUrlEncodedContent);
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                token = await JsonSerializer.DeserializeAsync<Classes.BusinessCentralToken>(
                    contentStream
                );
                _memoryCache.Set<Classes.BusinessCentralToken>(
                    "Token-BusinessCentral",
                    token!,
                    DateTimeOffset.Now.AddMinutes(58)
                );
            }
            else
            {
                throw new Exception("Error al obtener el Token de Business Central");
            }
        }
        return token!;
    }

}
