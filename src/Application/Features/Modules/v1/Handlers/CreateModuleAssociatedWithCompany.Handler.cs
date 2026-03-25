using MediatR;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Modules.v1.Commands;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Application.Features.Modules.v1.Handlers
{
    public class CreateModuleAssociatedWithCompanyHandler(IUnitOfWork _unitOfWork, ICodeGenerator _codeGenerator) : IRequestHandler<CreateModuleAssociatedWithCompanyCommand>
    {
        public async Task Handle(CreateModuleAssociatedWithCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _unitOfWork.Companies.FirstOrDefaultAsync(x => x.Id == request.CompanyId, cancellationToken);

            var codeGenerted = _codeGenerator.GenerateModuleCode(request.ModuleName!);

            if (company is null)
            {
                throw new Exception("No existe una empresa asociado a este id!");
            }

            await _unitOfWork.Modules.CreateModuleAssociatedWithCompany(new()
            {
                ModuleName = request.ModuleName!,
                CompanyId = request.CompanyId,
                Code = codeGenerted,
                Description = request.Description
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}