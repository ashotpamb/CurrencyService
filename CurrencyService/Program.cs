
using CurrencyService.Domain;
using CurrencyService.Services;
using ExchangeData.Data;
using ExchangeData.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IClient, SoapClient>();
builder.Services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
builder.Services.AddDbContext<DataContext>(options =>
{
    var connecttionString = builder.Configuration.GetConnectionString("SqlConnection");
    options.UseSqlServer(connecttionString);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
