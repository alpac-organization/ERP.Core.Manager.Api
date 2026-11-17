using MediatR;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;


namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class DeleteProductHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager) : BaseValidatorHandler<DeleteProductCommand, Unit>(_unitOfWork, _errorManager)
    {
        public override async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            if (access.Role?.RoleType == RoleType.Supervisor)
            {
                return _errorManager.ThrowUnauthorized<Unit>("No estas autorizado a realizar esta acción", "ERP:01");
            }

            var product = await _unitOfWork.Products.Entities
                .Where(product => product.ProductStatus == ProductStatus.Active)
                .Where(product => product.Id == request.ProductId)
                .Where(product => product.CostCenterId == access.Profile.CostCenterId)
                .Include(product => product.ProductStocks)
                .FirstOrDefaultAsync(cancellationToken);

            if (product is null)
            {
                return _errorManager.ThrowBadRequest<Unit>("Este producto no existe o no se encuentra activo", "ERP:01");
            }

            var stock = await _unitOfWork.ProductStocks.Entities
                .Include(product => product.Product)
                .Where(productStock => productStock.IsActive)
                .Where(productStock => productStock.ProductId == product.Id)
                .Where(productStock => productStock.Product.Code == product.Code)
                .Where(productStock => productStock.CostCenterId == access.Profile.CostCenterId)
                .FirstOrDefaultAsync(cancellationToken);

            if (stock is not null)
            {

                if (stock.AvailableQuantity <= 0)
                {
                    product.DeletedAt = DateTime.Today;
                    product.ProductStatus = ProductStatus.Inactive;

                    stock.IsActive = false;
                    stock.DeletedAt = DateTime.Now;
                }
                else
                {                    
                    product.ProductStatus = ProductStatus.LowProcess;
                    
                    stock.IsActive = false;
                    stock.DeletedAt = DateTime.Now;
                }


                await _unitOfWork.ProductStocks.UpdateAsync(stock);
            }
            else
            {
                product.DeletedAt = DateTime.Today;
                product.ProductStatus = ProductStatus.Inactive;
            }

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
   }
}