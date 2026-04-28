using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Incomes.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Incomes.v1.Handlers
{
    public class RegisterDeductionHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<RegisterIncomeCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(RegisterIncomeCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            if (access.Role!.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para registrar una dedución", "ERP:01");
            }
            


            return true; 
        }
    }
}