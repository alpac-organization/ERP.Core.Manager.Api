using AutoMapper;
using ERP.Core.Database.Domain.Entities.Inventory;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings;

public class ReasonsInventoryRemovalProfile : Profile
{
    public ReasonsInventoryRemovalProfile()
    {
        CreateMap<ReasonsInventoryRemoval, ReasonsInventoryRemovalDto>()
            .ForMember(dest => dest.ReasonsInventoryRemovalId, opt => opt.MapFrom(src => src.Id));

    }
}

public static class ReasonsInventoryRemovalMapper
{
    public static ReasonsInventoryRemoval ToReasonsInventoryRemovalEntity(this RegisterReasonsInventoryRemovalCommand command)
    {
        return new()
        {
            Code = command.Code.Trim().ToUpper(),
            Description = command.Description,
            IsActive = true
        };
    }
}
