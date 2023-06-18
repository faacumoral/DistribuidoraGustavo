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



builder.InitConfig();
builder.AddJwtManager();
builder.AddCors();

builder.Services.AddDbContext<DistribuidoraGustavoContext>(
    options => {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DistribuidoraGustavo"));
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("Origins");

app.MapControllers();

app.Run();
