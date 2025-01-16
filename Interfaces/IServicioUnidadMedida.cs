namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioUnidadMedida
{
    Task<List<Classes.LSCentralUnidadMedida>> ObtenerUnidadesMedida(string? filtro);

    Task<Classes.LSCentralUnidadMedida> ObtenerUnidadMedida(string id);

    Task<bool> RefrescarCache();
}
