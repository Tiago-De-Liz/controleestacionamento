using ControleEstacionamento.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleEstacionamento.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<VeiculoEstacionado> VeiculosEstacionados { get; set; } = null!;
    public DbSet<TabelaPreco> TabelasPreco { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
