using Bellon.API.ConsumoInterno.Interfaces;
using Microsoft.Azure.KeyVault.Models;

public class ServicioSegundoPlano : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ServicioSegundoPlano(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Crear un scope de servicios para poder resolver servicios scoped
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _servicioProducto = scope.ServiceProvider.GetRequiredService<IServicioProducto>();

                try
                {
                    // Ejecutar la lógica del servicio scoped
                    await _servicioProducto.ObtenerProductos();
                }
                catch (Exception ex)
                {
                    // Manejo de excepciones
                    Console.WriteLine($"Error al obtener productos: {ex.Message}");
                }
            }

            // Esperar 5 segundos entre ejecuciones
            await Task.Delay(5000, stoppingToken);
        }
    }

}
