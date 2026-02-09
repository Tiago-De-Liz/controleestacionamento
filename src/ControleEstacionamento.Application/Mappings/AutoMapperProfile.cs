using AutoMapper;
using ControleEstacionamento.Application.DTOs;
using ControleEstacionamento.Domain.Entities;

namespace ControleEstacionamento.Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<VeiculoEntradaDto, VeiculoEstacionado>()
            .ForMember(dest => dest.DataHoraEntrada, opt => opt.MapFrom(_ => DateTime.Now));
        CreateMap<VeiculoEstacionado, VeiculoResponseDto>();

        CreateMap<TabelaPreco, TabelaPrecoDto>();
        CreateMap<TabelaPrecoDto, TabelaPreco>();
        CreateMap<TabelaPrecoCreateDto, TabelaPreco>();
        CreateMap<TabelaPrecoUpdateDto, TabelaPreco>();
    }
}
