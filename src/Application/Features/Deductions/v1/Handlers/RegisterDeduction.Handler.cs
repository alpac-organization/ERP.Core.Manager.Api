using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Handlers
{
    public class RegisterDeductionHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<RegisterDeductionCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(RegisterDeductionCommand request, CancellationToken cancellationToken)
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


            
            //Verificar si hay una nomina en progreso y actualizamos sus datos en el sistema. si y solo si esta la nomina.


            return true; 
        }
    }
}