using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Handlers
{
    public class RegisterCustomerHandler(IUnitOfWork unitOfWork, IErrorManager errorManager) : BaseValidatorHandler<RegisterCustomerCommand, bool>(unitOfWork, errorManager)
    {
        public override async Task<bool> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            } 



            return true;
        }
    }
}