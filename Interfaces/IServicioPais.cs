namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioPais
{
    Task<List<Classes.LSCentralPais>> ObtenerPaises();

    Task<Classes.LSCentralPais> ObtenerPais(string id);

    Task<bool> RefrescarCache();
}
