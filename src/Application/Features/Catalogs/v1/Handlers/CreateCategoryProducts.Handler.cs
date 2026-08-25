using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers;

public class CreateCategoryHandler(
    IUnitOfWork _unitOfWork,
    ILogger<CreateCategoryHandler> logger,
    IErrorManager _errorManager)
    : BaseValidatorHandler<CreateCategoryCommand, Guid>(_unitOfWork, _errorManager)
{
    public override async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Validación de acceso
        var access = await ValidateAccessAsync(
            request.UserId,
            request.CompanyId,
            request.ModuleCode!,
            cancellationToken);

        if (!access.IsSuccess)
            return access.ErrorResponse;

        // Normalizar entradas
        var categoryName = request.Name?.Trim();
        var categoryCode = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim();

        // Validar existencia y actividad del padre (si se proporciona)
        if (request.ParentId.HasValue)
        {
            var parentExists = await _unitOfWork.CategoryProducts.Entities
                .AnyAsync(c => c.Id == request.ParentId.Value && c.IsActive, cancellationToken);

            if (!parentExists)
                return _errorManager.ThrowBadRequest<Guid>(
                    "La categoría padre seleccionada no existe o no está activa.",
                    "ERP:003");
        }

        // Verificar duplicados (nombre y código) - solo categorías activas
        var duplicateName = await _unitOfWork.CategoryProducts.Entities
            .AnyAsync(c => c.Name != null && c.Name == categoryName && c.IsActive, cancellationToken);

        if (duplicateName)
            return _errorManager.ThrowBadRequest<Guid>(
                "Ya existe una categoría activa con ese nombre.",
                "ERP:005");

        if (!string.IsNullOrWhiteSpace(categoryCode))
        {
            var duplicateCode = await _unitOfWork.CategoryProducts.Entities
                .AnyAsync(c => c.Code != null && c.Code == categoryCode && c.IsActive, cancellationToken);

            if (duplicateCode)
                return _errorManager.ThrowBadRequest<Guid>(
                    "Ya existe una categoría activa con ese código.",
                    "ERP:006");
        }

        logger.LogInformation("🚀 Iniciando registro de categoría: {CategoryName}", categoryName);

        var category = new CategoryProducts
        {
            Name = categoryName,
            Code = categoryCode,
            IsActive = request.IsActive,
            ParentId = request.ParentId   // ← Aquí se asigna el padre
        };

        // Si el repositorio no guarda el ParentId, prueba con Entities.Add
        await _unitOfWork.CategoryProducts.CreateCategoryProduct(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("✅ Categoría {CategoryId} registrada exitosamente", category.Id);

        return category.Id;
    }
}