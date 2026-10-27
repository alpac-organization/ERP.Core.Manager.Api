using AutoMapper;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Database.Domain.Entities.Warehouse;

using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos;

using Commands = ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Database.Domain.Entities.Inventory;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{    
    public class ProductsProfile : Profile
    {
        public ProductsProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.UsageType, opt => opt.MapFrom(src => src.ProductUsageType))
                .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.BranchId))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.UnitMeasureId, opt => opt.MapFrom(src => src.UnitMeasureId))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.SuppliersCount, opt => opt.MapFrom(src => src.SupplierProducts.Count))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => new ProductCategoryDto
                {
                    Name = src.Category.Name,
                    Code = src.Category.Code,
                    IsActive = src.Category.IsActive
                }))
                .ForMember(dest => dest.CostCenterInformation, opt => opt.MapFrom(src => src.CostCenter != null ? new CostCenterDto
                {
                    CostCenterId = src.CostCenter.Id,
                    CostCenterName = src.CostCenter.CostCenterName,
                    CostCenterCode = src.CostCenter.CostCenterCode
                } : null));

            CreateMap<CategoryProducts, CategoryProductDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.ParentId))
                .ForMember(dest => dest.SubCategory, opt => opt.MapFrom(src => src.Children.Where(c => c.IsActive).ToList()));
        }
    }
    
    public static class ProductMapper
    {
        public static Product ToProductEntity(this Commands.RegisterProductCommand command, Dictionary<Guid, Currency> supplierCurrencies)
        {
            return new ()
            {
                Id               = Guid.NewGuid(),
                ProductName      = command.ProductName,
                CategoryId       = command.CategoryId,
                Description      = command.Description,
                IsTaxExempt      = command.IsTaxExempt,
                UnitMeasureId    = command.UnitMeasureId,
                ProductUsageType = command.ProductUsageType,
                SupplierProducts =  [
                    ..command.Suppliers.Select(s => SupplierProductPriceHelper.BuildSupplierProduct(
                        s.SupplierId,
                        productId: null,
                        s.UnitPrice,
                        s.TierPrices,
                        s.UnitMeasureId ?? command.UnitMeasureId,
                        DateTime.Now,
                        s.Currency ?? supplierCurrencies.GetValueOrDefault(s.SupplierId, Currency.NIO))
                    )
                ]
            };
        }

        public static ProductStock ToProductStockEntity(this Commands.RegisterProductCommand command, Guid productId)
        {
            return new()
            {
                MaximumStock = command.ConfigurationStock!.MaximumStock,
                MinimumStock = command.ConfigurationStock.MinimumStock,
                UnitMeasureId = command.UnitMeasureId,
                WeightedAverageCost = 0.0m,
                Status = ProductStockStatus.Empty,
                ProductId = productId,
                AvailableQuantity = 0,
                IsActive = true
            };
        }
    }
}