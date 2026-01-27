using Microsoft.EntityFrameworkCore;
using ApiUp.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

var mySqlVersion = new MySqlServerVersion(new Version(5, 7, 39));


builder.Services.AddDbContext<UsersContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<ClassroomsContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<DirectionsContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<StatusesContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<EquipmentTypesContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<ModelsContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<EquipmentContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<EquipmentHistoryContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<NetworkSettingsContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<DevelopersContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<SoftwareContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<EquipmentSoftwareContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<InventoriesContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<InventoryItemsContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<ConsumableTypesContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<ConsumablesContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddDbContext<ConsumableCharacteristicsContext>(o =>
{
    o.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), mySqlVersion);
    o.EnableSensitiveDataLogging();
    o.EnableDetailedErrors();
});

builder.Services.AddControllers();
builder.Services.AddMvc(option => option.EnableEndpointRouting = true);


builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Version = "v1", Title = "GET запросы" });
    option.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo { Version = "v2", Title = "POST запросы" });
    option.SwaggerDoc("v3", new Microsoft.OpenApi.Models.OpenApiInfo { Version = "v3", Title = "PUT запросы" });
    option.SwaggerDoc("v4", new Microsoft.OpenApi.Models.OpenApiInfo { Version = "v4", Title = "DELETE запросы" });
});

var app = builder.Build();


app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"Response: {context.Response.StatusCode}");
});


app.Use(async (context, next) =>
{
    try { await next(); }
    catch (Exception ex)
    {
        Console.WriteLine($"Unhandled exception: {ex}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Internal Server Error");
    }
});


using (var scope = app.Services.CreateScope())
{
    var s = scope.ServiceProvider;

    s.GetRequiredService<UsersContext>().Database.EnsureCreated();
    s.GetRequiredService<ClassroomsContext>().Database.EnsureCreated();
    s.GetRequiredService<DirectionsContext>().Database.EnsureCreated();
    s.GetRequiredService<StatusesContext>().Database.EnsureCreated();
    s.GetRequiredService<EquipmentTypesContext>().Database.EnsureCreated();
    s.GetRequiredService<ModelsContext>().Database.EnsureCreated();

    s.GetRequiredService<EquipmentContext>().Database.EnsureCreated();
    s.GetRequiredService<EquipmentHistoryContext>().Database.EnsureCreated();
    s.GetRequiredService<NetworkSettingsContext>().Database.EnsureCreated();

    s.GetRequiredService<DevelopersContext>().Database.EnsureCreated();
    s.GetRequiredService<SoftwareContext>().Database.EnsureCreated();
    s.GetRequiredService<EquipmentSoftwareContext>().Database.EnsureCreated();

    s.GetRequiredService<InventoriesContext>().Database.EnsureCreated();
    s.GetRequiredService<InventoryItemsContext>().Database.EnsureCreated();

    s.GetRequiredService<ConsumableTypesContext>().Database.EnsureCreated();
    s.GetRequiredService<ConsumablesContext>().Database.EnsureCreated();
    s.GetRequiredService<ConsumableCharacteristicsContext>().Database.EnsureCreated();
}

app.UseSwagger();
app.UseRouting();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GET");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "POST");
    c.SwaggerEndpoint("/swagger/v3/swagger.json", "PUT");
    c.SwaggerEndpoint("/swagger/v4/swagger.json", "DELETE");
});

app.Run();
