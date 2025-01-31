namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioNumeroSerie
{
    Task<string> ObtenerNumeroDocumento(int id);
}
