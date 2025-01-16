namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioDocumentoOrigen
{

    Task<List<Classes.LSCentralDocumentoOrigen>> ObtenerFacturas(
        string filtroId,
        string filtroProveedor
    );

    Task<List<Classes.LSCentralDocumentoOrigen>> ObtenerRecepciones(string id);

    Task<Classes.LSCentralDocumentoOrigen> ObtenerFactura(string id, string noRecepcion);
}
