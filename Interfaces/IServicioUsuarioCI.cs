using Bellon.API.Liquidacion.Classes;

namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioUsuarioCI
{
    Task<List<Usuario>> ObtenerUsuarios();
    Task<Usuario> ObtenerUsuario(int? id);
    Task<Usuario> ObtenerUsuarioPorCorreo(string? correo);
    Task<List<Usuario>> ObtenerUsuarioResponsablesPorDepartamentos(string? departamentoId);
    Task<Usuario> GuardarUsuario(Usuario usuario);
    Task<Usuario> EliminarUsuario(int id);
    Task<bool> RefrescarCache();
}
