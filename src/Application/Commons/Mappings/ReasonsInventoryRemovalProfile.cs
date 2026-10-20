using AutoMapper;
using ERP.Core.Database.Domain.Entities.Inventory;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings;

public class ReasonsInventoryRemovalProfile : Profile
{
    public ReasonsInventoryRemovalProfile()
    {
        CreateMap<ReasonsInventoryRemoval, ReasonsInventoryRemovalDto>();
    }
}