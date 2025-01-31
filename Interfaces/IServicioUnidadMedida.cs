using Bellon.API.ConsumoInterno.Classes;

namespace Bellon.API.ConsumoInterno.Interfaces;
public interface IServicioUnidadMedida
{
    Task<List<Classes.LSCentralUnidadMedida>> ObtenerUnidadesMedida(string? filtro);

    Task<Classes.LSCentralUnidadMedida> ObtenerUnidadMedida(string id);

    Task<bool> RefrescarCache();
}
