using Microsoft.Extensions.Configuration;
using PriceLoaderApp.Configuration;
using PriceLoaderApp.Services;


var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var suppliers = config
    .GetSection("Suppliers")
    .Get<List<SupplierConfig>>()!;

var mail = new MailService(config);
var csv = new CsvProcessor();
var db = new DatabaseRepository(config);

var loader = new PriceLoaderService(mail, csv, db, suppliers);
await loader.RunAsync();

Console.WriteLine("Загрузка прайсов завершена.");
