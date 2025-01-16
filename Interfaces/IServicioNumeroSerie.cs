namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioNumeroSerie
{
    Task<string> ObtenerNumeroDocumento(int id);
}
