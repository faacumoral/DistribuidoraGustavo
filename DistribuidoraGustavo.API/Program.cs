using DistribuidoraGustavo.Core.Services;
using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IUserService, UserService>();

DistribuidoraGustavo.API.Configure.AddConfig(builder.Configuration);

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
