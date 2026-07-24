using AutoMapper;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Entities.Warehouse;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class RegisterProductHandler(
    IUnitOfWork _unitOfWork,
    ILogger<RegisterProductHandler> _logger,
    IErrorManager _errorManager,
    IMapper _mapper) 
    : BaseValidatorHandler<RegisterProductCommand, bool>(_unitOfWork, _errorManager)
{
    public override async Task<bool> Handle(RegisterProductCommand request, CancellationToken cancellationToken)
    {
        var access = await ValidateAccessAsync(
            request.UserId,
            request.CompanyId,
            request.ModuleCode!,
            cancellationToken);

        if (!access.IsSuccess)
        {
            return access.ErrorResponse;
        }   

        var categoryExists = await _unitOfWork.CategoryProducts.Entities
            .AnyAsync(c => c.Id == request.CategoryId && c.IsActive, cancellationToken);
        
        if (!categoryExists)
        {
            return _errorManager.ThrowBadRequest<bool>(
                "La categoría seleccionada no existe o no está activa.",
                "ERP:002");
        }
        
        _logger.LogInformation("🚀 Iniciando registro de producto: {ProductName}", request.ProductName);

        var productEntity = _mapper.Map<Product>(request);

        await _unitOfWork.Products.InsertProduct(productEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("✅ Producto {ProductId} registrado exitosamente", productEntity.Id);

        return true;
    }
}