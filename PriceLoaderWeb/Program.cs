using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PriceLoaderWeb.Configuration;
using PriceLoaderWeb.Data;
using PriceLoaderWeb.Services;
using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<PriceLoaderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

var appSettings = new AppSettings();
builder.Configuration.GetSection("Mail").Bind(appSettings.Mail);
appSettings.Suppliers = builder.Configuration.GetSection("Suppliers").Get<List<SupplierConfig>>() ?? new();

var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
logger.LogInformation("Loaded {Count} suppliers from configuration", appSettings.Suppliers.Count);
foreach (var s in appSettings.Suppliers)
{
    logger.LogInformation("Supplier: {Name}, Email: {Email}", s.Name, s.SenderEmail);
}

builder.Services.AddSingleton(appSettings);
builder.Services.AddSingleton<IEnumerable<SupplierConfig>>(appSettings.Suppliers);

builder.Services.AddScoped<IPriceRepository, PriceRepository>();
builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<ICsvProcessor, CsvProcessor>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IPriceLoaderService, PriceLoaderService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PriceLoaderDbContext>();
    var dbLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    dbLogger.LogInformation("Ensuring database is created...");
    var created = db.Database.EnsureCreated();
    dbLogger.LogInformation("Database created: {Created}", created);
    
    try
    {
        var tables = db.Database.SqlQueryRaw<string>(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'"
        ).ToList();
        dbLogger.LogInformation("Tables in database: {Tables}", string.Join(", ", tables));
    }
    catch (Exception ex)
    {
        dbLogger.LogError(ex, "Failed to get tables list");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=PriceLoader}/{action=Index}/{id?}");

app.Run();
