using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Handlers
{
    public class RegisterExtraordinaryPaymentHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<RegisterExtraordinaryPaymentCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(RegisterExtraordinaryPaymentCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            if (access.Role!.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para registrar este pago de dedución", "ERP:01");
            }
            

            return true; 
        }
    }
}