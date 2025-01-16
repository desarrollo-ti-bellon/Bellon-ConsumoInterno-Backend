namespace Bellon.API.Liquidacion.Interfaces
{
    public interface IServicioMovValorPrecios
    {
        Task<List<Classes.LSCentralMovValorPrecios>> ObtenerMovValorPrecios(string no_documento );
    }
}
