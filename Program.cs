using Bellon.API.Liquidacion.Authorization;
using Bellon.API.Liquidacion.Classes;
using Bellon.API.Liquidacion.Interfaces;
using Bellon.API.Liquidacion.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddDbContext<Bellon.API.Liquidacion.DataBase.AppDataBase>();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

//Third APIs
builder.Services.AddHttpClient(
    "BusinessCentral-Token",
    httpClient =>
    {
        httpClient.BaseAddress = new Uri(
            builder.Configuration["AppSettings:BusinessCentral-Token"]!
        );
    }
);
builder.Services.AddHttpClient(
    "BusinessCentral-APIs",
    httpClient =>
    {
        httpClient.BaseAddress = new Uri(
             builder.Configuration["AppSettings:BusinessCentral-APIs"]!
        );
    }
);
// ESTE ES SOLO PARA LAS APIS QUE PERTENECEN AL API GROUP DE LOS QUE SON GENERALES
builder.Services.AddHttpClient(
    "LSCentral-APIs",
    httpClient =>
    {
        httpClient.BaseAddress = new Uri(
             builder.Configuration["AppSettings:LSCentral-APIs"]!
        );
    }
);
// ESTE ES SOLO PARA LAS APIS QUE PERTENECEN AL API GROUP DE LIQUIDACION
builder.Services.AddHttpClient(
    "LSCentral-APIs-Liquidacion",
    httpClient =>
    {
        httpClient.BaseAddress = new Uri(
            builder.Configuration["AppSettings:LSCentral-APIs-Liquidacion"]!
        );
    }
);

//App Services
builder.Services.AddScoped<IServicioNumeroSerie, ServicioNumeroSerie>();
builder.Services.AddScoped<IServicioAgente, ServicioAgente>();
builder.Services.AddScoped<IServicioTipoContenedor, ServicioTipoContenedor>();
builder.Services.AddScoped<IServicioCodigoArancelario, ServicioCodigoArancelario>();
builder.Services.AddScoped<IServicioLlegada, ServicioLlegada>();
builder.Services.AddScoped<IServicioHistLlegada, ServicioHistLlegada>();
builder.Services.AddScoped<IServicioTransito, ServicioTransito>();
builder.Services.AddScoped<IServicioHistTransito, ServicioHistTransito>();
builder.Services.AddScoped<IServicioLiquidacion, ServicioLiquidacion>();
builder.Services.AddScoped<IServicioHistLiquidacion, ServicioHistLiquidacion>();
builder.Services.AddScoped<IServicioProveedor, ServicioProveedor>();
builder.Services.AddScoped<IServicioCategoriaProducto, ServicioCategoriaProducto>();
builder.Services.AddScoped<IServicioUsuario, ServicioUsuario>();
builder.Services.AddScoped<IServicioAlmacen, ServicioAlmacen>();
builder.Services.AddScoped<IServicioUnidadMedida, ServicioUnidadMedida>();
builder.Services.AddScoped<IServicioPais, ServicioPais>();
builder.Services.AddScoped<IServicioCargoProducto, ServicioCargoProducto>();
builder.Services.AddScoped<IServicioProducto, ServicioProducto>();
builder.Services.AddScoped<IServicioDocumentoOrigen, ServicioDocumentoOrigen>();
builder.Services.AddScoped<IServicioOrdenTransferencia, ServicioOrdenTransferencia>();
builder.Services.AddScoped<IServicioMovValorPrecios, ServicioMovValorPrecios>();
builder.Services.AddScoped<IServicioCargoFactura, ServicioCargoFactura>();
builder.Services.AddScoped<IServicioSolicitud, ServicioSolicitud>();
builder.Services.AddScoped<IServicioEstadoSolicitud, ServicioEstadoSolicitud>();
builder.Services.AddScoped<IServicioNotas, ServicioNotas>();
builder.Services.AddScoped<IServicioClasificacion, ServicioClasificacion>();
builder.Services.AddScoped<IServicioDepartamento, ServicioDepartamento>();
builder.Services.AddScoped<IServicioSucursal, ServicioSucursal>();
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
