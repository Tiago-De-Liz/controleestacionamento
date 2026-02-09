using ControleEstacionamento.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleEstacionamento.Infrastructure.Data.Configurations;

public class VeiculoEstacionadoConfiguration : IEntityTypeConfiguration<VeiculoEstacionado>
{
    public void Configure(EntityTypeBuilder<VeiculoEstacionado> builder)
    {
        builder.ToTable("VeiculosEstacionados");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Placa)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(v => v.DataHoraEntrada)
            .IsRequired();

        builder.Property(v => v.DataHoraSaida);

        builder.Property(v => v.ValorCobrado)
            .HasPrecision(10, 2);

        builder.HasIndex(v => v.Placa);
    }
}
