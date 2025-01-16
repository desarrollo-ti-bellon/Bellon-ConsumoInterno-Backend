namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioNotas
{
    Task<List<Classes.Notas>> ObtenerNotas();

    Task<Classes.Notas> ObtenerNota(int id);

    Task<List<Classes.Notas>> ObtenerNotasDelUsuario(string UsuarioDestino);

    Task<List<Classes.Notas>> ObtenerNotasPorParametros(string? UsuarioDestino, string? tipoDocumento);

    Task<int> ObtenerCantidadDeNotas(string UsuarioDestino);

    Task<List<Classes.Notas>> GuardarNotas(Classes.Notas item);

    Task<List<Classes.Notas>> EliminarNotas(int id);

    Task<bool> RefrescarCache(bool incluyeProduccion = false);
}
