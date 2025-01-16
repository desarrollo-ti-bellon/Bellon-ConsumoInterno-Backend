namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioAgente
{
    Task<List<Classes.Agente>> ObtenerAgentes();

    Task<List<Classes.Agente>> ObtenerAgentesActivos();

    Task<Classes.Agente> ObtenerAgente(int id);

    Task<Classes.Agente> GuardarAgente(Classes.Agente item);

    Task<bool> RefrescarCache();
}
