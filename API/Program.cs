
using API.Extensions;
using AspNetCoreRateLimit;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//SeriLog -> Configuración obtenida desde AppSettings
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

//agregamos SeriLog al sistema integrado de Loggin
//builder.Logging.ClearProviders(); //limpiar los proveedores por defecto
builder.Logging.AddSerilog(logger: logger);

builder.Services.ConfigureCors(); //desde mi extension
builder.Services.AddAplicationServices(); // Inyección de dependencias
builder.Services.AddAutoMapper(Assembly.GetEntryAssembly());
builder.Services.ConfigureRateLimiting();
builder.Services.ConfigureApiVersioning();
//JWT
builder.Services.AddJwt(configuration: builder.Configuration);


//configuración para serializar respuestas en XML
//Por defecto resolverá en Application/json
builder.Services.AddControllers(opt => {
    opt.RespectBrowserAcceptHeader = true; //aceptar headers del browser
    opt.ReturnHttpNotAcceptable = true; //devolver un mensaje de error indicando que el formato solicitado no es aceptado que soporta el servidor
}).AddXmlDataContractSerializerFormatters();

builder.Services.AddDbContext<TiendaContext>(opt =>
{
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 30));
    opt.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), serverVersion);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


app.UseIpRateLimiting();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//iniciar DB si existen migraciones pendientes
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<TiendaContext>();
        await context.Database.MigrateAsync();
        await TiendaContextSeed.SeedAsync(context, loggerFactory);
        await TiendaContextSeed.SeedRolesAsync(context, loggerFactory);
    }
    catch (Exception ex)
    {
        var _logger = loggerFactory.CreateLogger<Program>();
        _logger.LogError(ex, "Ocurrió un error durante la migración");
    }
}

app.UseCors("CorsPolicy"); //nombre de mi política

app.UseHttpsRedirection();

//siempre debe ir antes que authorization
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
