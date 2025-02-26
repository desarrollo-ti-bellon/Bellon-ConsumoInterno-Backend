namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioProducto
{

    Task<List<Classes.LSCentralExistenciaProducto>> DisponibilidadProducto(string codigoAlmacen);

    Task<List<Classes.LSCentralProducto>> ObtenerProductos();

    Task<Classes.LSCentralProducto> ObtenerProducto(string id);

    Task<List<Classes.LSCentralTraducciones>> ObtenerTraduccionesProductos();

    Task<List<Classes.LSCentralTraducciones>> ObtenerTraduccionesProductosPorIds(List<Guid> ids);

    Task<bool> RefrescarCache();

}
