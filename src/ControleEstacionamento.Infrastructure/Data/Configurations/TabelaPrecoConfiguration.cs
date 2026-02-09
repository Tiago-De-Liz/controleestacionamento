using ControleEstacionamento.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleEstacionamento.Infrastructure.Data.Configurations;

public class TabelaPrecoConfiguration : IEntityTypeConfiguration<TabelaPreco>
{
    public void Configure(EntityTypeBuilder<TabelaPreco> builder)
    {
        builder.ToTable("TabelasPreco");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.DataInicioVigencia)
            .IsRequired();

        builder.Property(t => t.DataFimVigencia)
            .IsRequired();

        builder.Property(t => t.ValorHoraInicial)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(t => t.ValorHoraAdicional)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.HasIndex(t => new { t.DataInicioVigencia, t.DataFimVigencia });
    }
}
