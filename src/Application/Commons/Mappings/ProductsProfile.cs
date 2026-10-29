using AutoMapper;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Database.Domain.Entities.Warehouse;

using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

using Commands = ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

using ERP.Core.Database.Domain.Entities.Inventory;
using ERP.Core.Database.Domain.Entities.Shopping;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{    
    public class ProductsProfile : Profile
    {
        public ProductsProfile()
        {
            DateOnly today = default;

            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.UsageType, opt => opt.MapFrom(src => src.ProductUsageType))
                .ForMember(dest => dest.BranchInformation, opt => opt.MapFrom(src => src.Branch))
                .ForMember(dest => dest.CostCenterInformation, opt => opt.MapFrom(src => src.CostCenter))
                .ForMember(dest => dest.UnitMeasureInformation, opt => opt.MapFrom(src => src.UnitMeasure))
                .ForMember(dest => dest.SuppliersCount, opt => opt.MapFrom(src => src.SupplierProducts.Count));

            CreateMap<Product, ProductDetailsDto>()
                .IncludeBase<Product, ProductDto>();
                
            CreateMap<CategoryProducts, CategoryProductDto>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Id));

            // SupplierProduct
            CreateMap<SupplierProduct, ProductSupplierDto>()
                .ForMember(d => d.SupplierProductId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.SupplierLegalName, o => o.MapFrom(s => s.Supplier.SuppliersLegalName))
                .ForMember(d => d.CommercialName, o => o.MapFrom(s => s.Supplier.CommercialName))
                .ForMember(d => d.SupplierType, o => o.MapFrom(s => s.Supplier.SupplierDetails != null
                        ? s.Supplier.SupplierDetails.SupplierType
                        : SupplierType.Ordinary
                    )
                )
                .ForMember(d => d.TierPrices, o => o.MapFrom(s =>
                    s.TierPrices
                        .Where(t => t.DeletedAt == null
                            && t.ValidFrom <= today
                            && (t.ValidTo == null || t.ValidTo >= today)
                        )
                        .OrderBy(t => t.ValidFrom)
                        .Take(1)
                    )
                );

            CreateMap<SupplierProductTierPrice, TierPriceResponseDto>()
                .ForMember(d => d.TierPriceId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.Currency, o => o.MapFrom(s => s.SupplierProduct.Currency));
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