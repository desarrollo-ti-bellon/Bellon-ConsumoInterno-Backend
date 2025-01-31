namespace Bellon.API.ConsumoInterno.Interfaces;

public interface IServicioCategoriaProducto
{
    Task<List<Classes.LSCentralCategoriaProducto>> ObtenerCategoriasProducto();

    Task<Classes.LSCentralCategoriaProducto> ObtenerCategoriaProducto(string id);

    Task<bool> RefrescarCache();
}
