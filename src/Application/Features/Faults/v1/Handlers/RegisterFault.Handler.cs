using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Handlers
{
   public class RegisterFaultHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager)
      : BaseValidatorHandler<RegisterFaultCommand, bool>(_unitOfWork, _errorManager)
   {
      public override async Task<bool> Handle(RegisterFaultCommand request, CancellationToken cancellationToken)
      {
         var access = await ValidateAccessAsync(
                     request.UserId,
                     request.CompanyId,
                     request.ModuleCode,
                     cancellationToken
                  );

         if (!access.IsSuccess)
         {
            return access.ErrorResponse!;
         }




         return true;
      }
   }
}