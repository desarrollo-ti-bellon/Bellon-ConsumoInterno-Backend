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

            /* COMENTÉ ESTE CODIGO POR QUE NECESITO QUE ESTE OBJETO ME TRAIGA TAMBIEN EL PERFIL DEL USUARIO
                cache = await _context
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
                        Estado = i.estado
                    })
                    .ToListAsync();
                _memoryCache.Set<List<Usuario>>(
                    "UsuariosCI",
                    cache,
                    DateTimeOffset.Now.AddMinutes(5)
                );
            */

            var usuarios = await _context.Usuarios
                .Select(i => new
                {
                    i.id_usuario_ci,
                    i.id_usuario,
                    i.nombre_usuario,
                    i.correo,
                    i.codigo_sucursal,
                    i.id_sucursal,
                    i.codigo_departamento,
                    i.id_departamento,
                    i.limite,
                    i.posicion_id,
                    i.estado,
                    i.posicion
                })
                .ToListAsync();

            cache = usuarios.Select(i => new Usuario
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
                Posicion = new Posicion
                {
                    PosicionId = i.posicion.posicion_id,
                    Descripcion = i.posicion.descripcion,
                    CrearSolicitud = i.posicion.crear_solicitud,
                    EnviarSolicitud = i.posicion.enviar_solicitud,
                    RegistrarSolicitud = i.posicion.registrar_solicitud,
                    AprobarRechazarSolicitud = i.posicion.aprobar_rechazar_solicitud,
                    VerSolicitudes = i.posicion.ver_solicitudes
                }
            }).ToList();

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

    public async Task<Usuario> ObtenerUsuarioPorCorreo(string? correo)
    {
        var allItems = await ObtenerUsuarios();
        return allItems.Where(i => i.Correo == correo).FirstOrDefault().Clone();
    }

    public async Task<List<Usuario>> ObtenerUsuarioResponsablesPorDepartamentos(string? departamentoId)
    {
        List<int> posiciones = new List<int> { 2, 3, 4 };
        var allItems = await ObtenerUsuarios();
        return allItems
            .Where(i => posiciones.Contains(i.PosicionId) && i.IdDepartamento == departamentoId)
            .OrderByDescending(i => i.PosicionId).ToList();
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

                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al actualizar el registro: <" + ex.Message + ">");
                }

                await RefrescarCache();
                return await ObtenerUsuario(oldItem.id_usuario_ci);
            }
        }
        else
        {
            var newItem = new DataBase.Usuarios
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
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el registro: <" + ex.Message + ">");
            }

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
