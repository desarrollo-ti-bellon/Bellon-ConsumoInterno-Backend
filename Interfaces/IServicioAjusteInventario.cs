using Bellon.API.ConsumoInterno.Classes;

namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioAjusteInventario
{
    Task<List<LSCentralAjusteInventario>> ObtenerAjustesDeInventarios();

    Task<LSCentralAjusteInventario> ObtenerAjusteDeInventario(string no_documento);

    Task<Resultado> CrearAjusteInventario(int? idSolicitud);

}
