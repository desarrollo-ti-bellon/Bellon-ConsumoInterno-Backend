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

public class ServicioUsuarioCI : IServicioUsuarioCI
{
    private readonly DataBase.AppDataBase _context;
    private readonly AppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    IHttpContextAccessor _httpContextAccessor;
    private readonly IServicioAutorizacion _servicioAutorizacion;

    public ServicioUsuarioCI(
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

    public async Task<List<Usuario>> ObtenerUsuarios()
    {
        var cache = _memoryCache.Get<List<Usuario>>("UsuariosCI");
        if (cache == null)
        {
            cache = _context
                .Usuarios.Select(i => new Usuario
                {
                    IdUsuarioCI = i.id_usuario_ci,
                    IdUsuario = i.id_usuario,
                    NombreUsuario = i.nombre_usuario,
                    Correo = i.correo,
                    CodigoSucursal = i.codigo_sucursal,
                    IdSucursal = i.id_sucursal,
                    CodigoDepartamento = i.codigo_departamento,
                    IdDepartamento = i.id_departamento,
                    Limite = i.limite,
                    PosicionId = i.posicion_id,
                    Estado = i.estado,
                })
                .ToList();
            _memoryCache.Set<List<Usuario>>(
                "UsuariosCI",
                cache,
                DateTimeOffset.Now.AddMinutes(5)
            );
        }
        return cache;
    }

    public async Task<Usuario> ObtenerUsuario(int? id)
    {
        var allItems = await ObtenerUsuarios();
        return allItems.Where(i => i.IdUsuarioCI == id).FirstOrDefault().Clone();
    }

    public async Task<Usuario> GuardarUsuario(Usuario item)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (item.IdUsuarioCI.HasValue)
        {
            var oldItem = _context
                .Usuarios.Where(i =>
                    i.id_usuario_ci == item.IdUsuarioCI.Value
                )
                .FirstOrDefault();
            if (oldItem != null)
            {
                oldItem.id_usuario = item.IdUsuario;
                oldItem.nombre_usuario = item.NombreUsuario;
                oldItem.correo = item.Correo;
                oldItem.codigo_sucursal = item.CodigoSucursal;
                oldItem.id_sucursal = item.IdSucursal;
                oldItem.codigo_departamento = item.CodigoDepartamento;
                oldItem.id_departamento = item.IdDepartamento;
                oldItem.limite = item.Limite;
                oldItem.posicion_id = item.PosicionId;
                oldItem.estado = item.Estado;

                _context.Usuarios.Add(oldItem);
                _context.SaveChanges();
                await RefrescarCache();
                return await ObtenerUsuario(oldItem.id_usuario_ci);
            }
        }
        else
        {
            var newItem = new DataBase.Models.Usuarios
            {
                id_usuario = item.IdUsuario,
                nombre_usuario = item.NombreUsuario,
                correo = item.Correo,
                codigo_sucursal = item.CodigoSucursal,
                id_sucursal = item.IdSucursal,
                codigo_departamento = item.CodigoDepartamento,
                id_departamento = item.IdDepartamento,
                limite = item.Limite,
                posicion_id = item.PosicionId,
                estado = item.Estado,
            };

            _context.Usuarios.Add(newItem);
            _context.SaveChanges();
            await RefrescarCache();
            return await ObtenerUsuario(newItem.id_usuario_ci);
        }
        return null;
    }

    public async Task<Usuario> EliminarUsuario(int id)
    {
        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        var oldItem = _context.Usuarios.Where(i => i.id_usuario_ci == id).FirstOrDefault();
        if (oldItem != null)
        {
            _context.Usuarios.Remove(oldItem);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el registro: <" + ex.Message + ">");
            }
            await RefrescarCache();
            return await ObtenerUsuario(oldItem.id_usuario_ci);
        }
        return null;
    }

    public async Task<bool> RefrescarCache()
    {
        _memoryCache.Remove("UsuariosCI");
        await ObtenerUsuarios();
        return true;
    }

}
