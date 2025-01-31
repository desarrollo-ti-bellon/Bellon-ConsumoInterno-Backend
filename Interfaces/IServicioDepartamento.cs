namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioDepartamento
{
    Task<List<Classes.LSCentralDepartamento>> ObtenerDepartamentos();

    Task<Classes.LSCentralDepartamento> ObtenerDepartamento(string id);

    Task<bool> RefrescarCache();
}
