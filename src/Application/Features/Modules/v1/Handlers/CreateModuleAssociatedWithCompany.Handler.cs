using MediatR;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Modules.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Modules.v1.Handlers
{
    public class CreateModuleAssociatedWithCompanyHandler(IUnitOfWork _unitOfWork, ICodeGenerator _codeGenerator, IErrorManager _errorManager) : IRequestHandler<CreateModuleAssociatedWithCompanyCommand, bool>
    {
        public async Task<bool> Handle(CreateModuleAssociatedWithCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _unitOfWork.Companies.FirstOrDefaultAsync(x => x.Id == request.CompanyId, cancellationToken);

            var codeGenerted = _codeGenerator.GenerateModuleCode(request.ModuleName!);

            if (company is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Esta empresa no existe en nuestro sistema", "ERP:001");
            }

            await _unitOfWork.Modules.CreateModuleAssociatedWithCompany(new()
            {
                ModuleName = request.ModuleName!,
                Code = codeGenerted,
                Description = request.Description,
                IsActive = true
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}