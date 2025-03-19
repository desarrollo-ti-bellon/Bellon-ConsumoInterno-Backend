using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Bellon.API.ConsumoInterno.Authorization;
using Bellon.API.ConsumoInterno.Classes;
using Bellon.API.ConsumoInterno.Interfaces;
using Bellon.API.ConsumoInterno.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);
AppSettings appSettings = new AppSettings();
if (builder.Environment.IsDevelopment())
{

    //********************************************  LOCALHOST  ****************************************************************
    //SI ES AMBIENTE DE DESARROLLO (LOCALHOST) LOS VALORES SE LEEN DE AQUI
    appSettings = new AppSettings
    {
        AplicacionId = 4,
        AplicacionUsuarioId = 7,
        DocumentoConsumoInternoNoSerieId = 1,
        CantidadDigitosDocumento = 8,
        LSCentralTokenClientSecret = "HH~8Q~25I9fMYRw46EIIveAuyWGZnCwtGvbH.aLo",
        LSCentralTokenUrl =
            "https://login.microsoftonline.com/a5aba6fb-8964-45ce-835a-20614cc908d3/oauth2/v2.0/token",
        LSCentralAPIsComunes =
            "https://api.businesscentral.dynamics.com/v2.0/Sandbox3/api/bellon/general/v1.0/companies(c76b3d61-0b81-ef11-ac23-6045bd3820c8)/",
        LSCentralQueryComunes = "https://api.businesscentral.dynamics.com/v2.0/a5aba6fb-8964-45ce-835a-20614cc908d3/Sandbox3/ODataV4/",
        DataBaseConnection =
            "Server=tcp:bellonapps.database.windows.net,1433;Initial Catalog=bellonapps;Persist Security Info=False;User ID=bellonadmin;Password=B3ll0nD4t4B4s3;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
    };
    //*************************************************************************************************************************
}
else
{
    //******************************************  AZURE (PRODUCCION Y DESARROLLO)  ********************************************
    //SI ES AMBIENTE DE AZURE (DESARROLLO O PRODUCCION) LOS VALORES SE LEEN DEL SERVICIO DE KEYVAULT DE AZURE
    try
    {
        SecretClientOptions options = new SecretClientOptions()
        {
            Retry =
            {
                Delay = TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(16),
                MaxRetries = 5,
                Mode = RetryMode.Exponential,
            },
        };
        var client = new SecretClient(
            new Uri(builder.Configuration["AzureKeyVault:Url"]!),
            new DefaultAzureCredential(),
            options
        );

        appSettings = new AppSettings
        {
            AplicacionId = Convert.ToInt32(
                ((KeyVaultSecret)client.GetSecret("ConsumoInterno-AplicacionId")).Value
            ),
            AplicacionUsuarioId = Convert.ToInt32(
                ((KeyVaultSecret)client.GetSecret("AdminUsuarios-AplicacionId")).Value
            ),
            DocumentoConsumoInternoNoSerieId = Convert.ToInt32(
                (
                    (KeyVaultSecret)client.GetSecret("ConsumoInterno-DocumentoConsumoInternoNoSerieId")
                ).Value
            ),
            CantidadDigitosDocumento = Convert.ToInt32(
                ((KeyVaultSecret)client.GetSecret("ConsumoInterno-CantidadDigitosDocumento")).Value
            ),
            LSCentralTokenClientSecret = (
                (KeyVaultSecret)client.GetSecret("Comun-LSCentralTokenClientSecret")
            ).Value,
            LSCentralTokenUrl = ((KeyVaultSecret)client.GetSecret("Comun-LSCentralTokenUrl")).Value,
            LSCentralAPIsComunes = (
                (KeyVaultSecret)client.GetSecret("Comun-LSCentralAPIUrl")
            ).Value,
            LSCentralQueryComunes = ((KeyVaultSecret)client.GetSecret("Comun-LSCentralQueryAPIs")).Value,
            DataBaseConnection = (
                (KeyVaultSecret)client.GetSecret("Comun-DataBaseConnection")
            ).Value,
        };
    }
    catch (Exception ex)
    {
        appSettings = new AppSettings { LSCentralTokenUrl = ex.Message };
    }
    //*************************************************************************************************************************
}
builder.Services.Configure<AppSettings>(options =>
{
    options.AplicacionId = appSettings!.AplicacionId;
    options.AplicacionUsuarioId = appSettings!.AplicacionUsuarioId;
    options.DocumentoConsumoInternoNoSerieId = appSettings.DocumentoConsumoInternoNoSerieId;
    options.CantidadDigitosDocumento = appSettings.CantidadDigitosDocumento;
    options.LSCentralTokenUrl = appSettings.LSCentralTokenUrl;
    options.LSCentralTokenClientSecret = appSettings.LSCentralTokenClientSecret;
    options.LSCentralAPIsComunes = appSettings.LSCentralAPIsComunes;
    options.LSCentralQueryComunes = appSettings.LSCentralQueryComunes;
    options.DataBaseConnection = appSettings.DataBaseConnection;
});

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Authentication
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddTransient<IServicioAutorizacion, ServicioAutorizacion>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<Bellon.API.ConsumoInterno.DataBase.AppDataBase>();

if (!string.IsNullOrEmpty(appSettings.LSCentralTokenUrl))
{
    builder.Services.AddHttpClient(
        "LSCentral-Token",
        httpClient =>
        {
            httpClient.BaseAddress = new Uri(appSettings.LSCentralTokenUrl);
        }
    );
}
if (!string.IsNullOrEmpty(appSettings.LSCentralAPIsComunes))
{
    builder.Services.AddHttpClient(
        "LSCentral-APIs-Comunes",
        httpClient =>
        {
            httpClient.BaseAddress = new Uri(appSettings.LSCentralAPIsComunes);
        }
    );
}

//App Services
builder.Services.AddScoped<IServicioNumeroSerie, ServicioNumeroSerie>();
builder.Services.AddScoped<IServicioAlmacen, ServicioAlmacen>();
builder.Services.AddScoped<IServicioPais, ServicioPais>();
builder.Services.AddScoped<IServicioProducto, ServicioProducto>();
builder.Services.AddScoped<IServicioProveedor, ServicioProveedor>();
builder.Services.AddScoped<IServicioUnidadMedida, ServicioUnidadMedida>();
builder.Services.AddScoped<IServicioSolicitud, ServicioSolicitud>();
builder.Services.AddScoped<IServicioConsumoInterno, ServicioConsumoInterno>();
builder.Services.AddScoped<IServicioHistorialMovimientosSolicitudes, ServicioHistoricoMovimientoSolicitud>();
builder.Services.AddScoped<IServicioEstadoSolicitud, ServicioEstadoSolicitud>();
builder.Services.AddScoped<IServicioNotas, ServicioNotas>();
builder.Services.AddScoped<IServicioClasificacion, ServicioClasificacion>();
builder.Services.AddScoped<IServicioDepartamento, ServicioDepartamento>();
builder.Services.AddScoped<IServicioSucursal, ServicioSucursal>();
builder.Services.AddScoped<IServicioUsuarioCI, ServicioUsuarioCI>();
builder.Services.AddScoped<IServicioUsuario, ServicioUsuario>();
builder.Services.AddScoped<IServicioPosicion, ServicioPosicion>();
builder.Services.AddScoped<IServicioAjusteInventario, ServicioAjusteInventario>();
builder.Services.AddScoped<IServicioDatosCache, ServicioDatosCache>();

// INYECTANDO LAS VISTAS
builder.Services.AddScoped<IServicioImpresionConsumoInterno, ServicioImpresionConsumoInternos>();

// INYECTANDO SERVICIO DE SEGUNDO PLANO
builder.Services.AddHostedService<ServicioSegundoPlano>();

builder.Services.AddMemoryCache();
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();

//}

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
