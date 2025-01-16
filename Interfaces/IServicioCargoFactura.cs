
namespace Bellon.API.Liquidacion.Interfaces;

public interface IServicioCargoFactura
{
    Task<List<Classes.CargoFacturaLiquidacion>> ObtenerCargoFacturas();

    Task<List<Classes.CargoFacturaLiquidacion>> ObtenerCargoFactura(int id);

    Task<List<Classes.CargoFacturaLiquidacion>> GuardarCargoFactura(List<Classes.CargoFacturaLiquidacion> items);

    Task<List<Classes.LSCentralCargoFactura>> ObtenerCargoFacturasERP();

    Task<Classes.LSCentralCargoFacturaArray> ObtenerCargoFacturaERP(string id);

    Task<List<Classes.CargoFacturaLiquidacion>> EliminarLineaCargoFactura(int id);

    Task<bool> RefrescarCache();
}
