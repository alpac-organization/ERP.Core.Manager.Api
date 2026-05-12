using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Loans.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Loans.v1.Handlers
{
    public class RegisterLoanHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<RegisterLoanCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(RegisterLoanCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }
            

            return true;       
        }
    }
}