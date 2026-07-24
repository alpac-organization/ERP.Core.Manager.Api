using AutoMapper;
using ERP.Core.Database.Domain.Entities.Shopping;
using ERP.Core.Manager.Api.Application.Features.Shopping.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings;

public class QuoteDetailProfile : Profile
{
    public QuoteDetailProfile()
    {
        CreateMap<QuoteDetail, QuoteDetailDto>()
            .ForMember(dest => dest.QuoteDetailId, opt => opt.MapFrom(src => src.Id));
    }
}