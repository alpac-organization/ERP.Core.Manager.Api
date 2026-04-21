using AutoMapper;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Application.Features.Branches.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Companies.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class BranchesProfile : Profile
    {
        public BranchesProfile()
        {
            CreateMap<Branch, BranchesDto>()
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.Id));
        }
    }
}