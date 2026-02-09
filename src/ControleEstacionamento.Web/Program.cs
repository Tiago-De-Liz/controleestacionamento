using ControleEstacionamento.Application.Interfaces;
using ControleEstacionamento.Application.Mappings;
using ControleEstacionamento.Application.Services;
using ControleEstacionamento.Application.Services.Strategies;
using ControleEstacionamento.Application.Validators;
using ControleEstacionamento.Domain.Interfaces;
using ControleEstacionamento.Infrastructure.Data;
using ControleEstacionamento.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=estacionamento.db"));

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<VeiculoEntradaValidator>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<ICalculoPrecoStrategy, CalculoPrecoStrategy>();
builder.Services.AddScoped<IEstacionamentoService, EstacionamentoService>();
builder.Services.AddScoped<ITabelaPrecoService, TabelaPrecoService>();

builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();

    if (app.Environment.IsDevelopment())
    {
        ControleEstacionamento.Infrastructure.Data.DbSeeder.Seed(dbContext);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Controle de Estacionamento API v1");
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
