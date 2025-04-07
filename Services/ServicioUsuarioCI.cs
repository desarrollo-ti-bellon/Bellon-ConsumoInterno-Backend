using System.Data.Common;
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

    public async Task<List<UsuarioCI>> ObtenerUsuarios()
    {
        var cache = _memoryCache.Get<List<UsuarioCI>>("UsuariosCI");
        if (cache == null)
        {
            var usuarios = await _context.UsuariosCI
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
                    i.codigo_almacen,
                    i.id_almacen,
                    i.posicion
                })
                .ToListAsync();

            cache = usuarios.Select(i => new UsuarioCI
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
                IdAlmacen = i.id_almacen,
                CodigoAlmacen = i.codigo_almacen,
                PosicionUsuarioCI = new PosicionUsuarioCI
                {
                    PosicionId = i.posicion.posicion_id,
                    Descripcion = i.posicion.descripcion,
                    CrearSolicitud = i.posicion.crear_solicitud,
                    EnviarSolicitud = i.posicion.enviar_solicitud,
                    AprobarSolicitud = i.posicion.aprobar_solicitud,
                    RechazarSolicitud = i.posicion.rechazar_solicitud,
                    ConfirmarSolicitud = i.posicion.confirmar_solicitud,
                    EntregarSolicitud = i.posicion.entregar_solicitud,
                    CancelarSolicitud = i.posicion.cancelar_solicitud
                }
            }).ToList();

            _memoryCache.Set<List<UsuarioCI>>(
                "UsuariosCI",
                cache,
                DateTimeOffset.Now.AddMinutes(5)
            );

        }
        return cache;
    }

    public async Task<List<UsuarioCI>> ObtenerUsuariosActivos()
    {
        var allItems = await ObtenerUsuarios();
        return allItems.Where(i => i.Estado == true).ToList();
    }

    public async Task<UsuarioCI> ObtenerUsuario(int? id)
    {

        if (id.HasValue == false || id == 0)
        {
            throw new InvalidDataException("El id del usuario no puede ser nulo o vacío");
        }

        var allItems = await ObtenerUsuarios();
        return allItems.Where(i => i.IdUsuarioCI == id).FirstOrDefault().Clone();
    }

    public async Task<UsuarioCI> ObtenerUsuarioPorCorreo(string? correo)
    {

        if (string.IsNullOrEmpty(correo))
        {
            throw new InvalidDataException("El correo del usuario no puede ser nulo");
        }

        var allItems = await ObtenerUsuariosActivos();
        return allItems.Where(i => i.Correo == correo && i.Estado == true).FirstOrDefault().Clone();
    }

    public async Task<List<UsuarioCI>> ObtenerUsuarioResponsablesPorDepartamentos(string? departamentoId)
    {

        if (string.IsNullOrEmpty(departamentoId))
        {
            throw new InvalidDataException("El id del departamento no puede ser nulo");
        }

        List<int> posiciones = new List<int> {
            2, // Director
            3, // Gerente Area
        };
        var allItems = await ObtenerUsuariosActivos();
        return allItems
            .Where(i => posiciones.Contains(i.PosicionId) && i.IdDepartamento == departamentoId)
            .OrderByDescending(i => i.PosicionId).ToList();
    }

    public async Task<UsuarioCI> GuardarUsuario(UsuarioCI item)
    {

        if (item == null)
        {
            throw new InvalidDataException("El objeto usuario no puede ser nulo o vacío");
        }

        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        if (item.IdUsuarioCI.HasValue)
        {
            var oldItem = _context
                .UsuariosCI.Where(i =>
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
                oldItem.id_almacen = item.IdAlmacen;
                oldItem.codigo_almacen = item.CodigoAlmacen;

                try
                {
                    _context.SaveChanges();
                }
                catch (DbException ex)
                {
                    throw new Exception(ex.Message);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException("Error al actualizar el registro: <" + ex.Message + ">");
                }

                await RefrescarCache();
                return await ObtenerUsuario(oldItem.id_usuario_ci);
            }
        }
        else
        {
            var newItem = new DataBase.UsuariosCI
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
                id_almacen = item.IdAlmacen,
                codigo_almacen = item.CodigoAlmacen
            };

            _context.UsuariosCI.Add(newItem);
            try
            {
                _context.SaveChanges();
            }
            catch (DbException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Error al actualizar el registro: <" + ex.Message + ">");
            }

            await RefrescarCache();
            return await ObtenerUsuario(newItem.id_usuario_ci);
        }
        return null;
    }

    public async Task<UsuarioCI> EliminarUsuario(int id)
    {

        if (id == 0 || id == null)
        {
            throw new InvalidDataException("El id del usuario no puede ser nulo o vacío");
        }

        var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        var oldItem = _context.UsuariosCI.Where(i => i.id_usuario_ci == id).FirstOrDefault();
        if (oldItem != null)
        {
            _context.UsuariosCI.Remove(oldItem);
            try
            {
                _context.SaveChanges();
            }
            catch (DbException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Error al eliminar el registro: <" + ex.Message + ">");
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
