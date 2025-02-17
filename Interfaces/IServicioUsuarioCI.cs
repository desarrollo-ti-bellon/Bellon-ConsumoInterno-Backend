using Bellon.API.ConsumoInterno.Classes;

namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioUsuarioCI
{
    Task<List<UsuarioCI>> ObtenerUsuarios();
    Task<UsuarioCI> ObtenerUsuario(int? id);
    Task<UsuarioCI> ObtenerUsuarioPorCorreo(string? correo);
    Task<List<UsuarioCI>> ObtenerUsuarioResponsablesPorDepartamentos(string? departamentoId);
    Task<UsuarioCI> GuardarUsuario(UsuarioCI usuario);
    Task<UsuarioCI> EliminarUsuario(int id);
    Task<bool> RefrescarCache();
}
