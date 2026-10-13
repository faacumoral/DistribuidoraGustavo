using DistribuidoraGustavo.Core.Services;
using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using DistribuidoraGustavo.API;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IInvoiceService, InvoiceService>();
builder.Services.AddTransient<IClientService, ClientService>();
builder.Services.AddTransient<IPriceListService, PriceListService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<IFamilyService, FamilyService>();
builder.Services.AddTransient<IPdfService, PdfService>();
builder.Services.AddTransient<IExcelService, ExcelService>();
builder.Services.AddTransient<ITransactionService, TransactionService>();

builder.InitConfig();
builder.AddJwtManager();

builder.Services.AddDbContext<DistribuidoraGustavoContext>(
    options => {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DistribuidoraGustavo"));
    });

var app = builder.Build();

// Apply pending EF Core migrations on startup (a failure stops the app instead of running on an outdated schema)
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DistribuidoraGustavoContext>().Database.Migrate();
}

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Blazor WASM (DistribuidoraGustavo.App) is served by this same host
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// unknown api routes must not fall back to the Blazor app
app.Map("api/{**path}", () => Results.NotFound());
app.MapFallbackToFile("index.html");

app.Run();
