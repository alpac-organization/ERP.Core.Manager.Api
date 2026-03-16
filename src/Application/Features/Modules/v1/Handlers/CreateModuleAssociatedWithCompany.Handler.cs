using MediatR;
using AutoMapper;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Modules.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Modules.v1.Handlers
{
    public class CreateModuleAssociatedWithCompanyHandler(IUnitOfWork _unitOfWork) : IRequestHandler<CreateModuleAssociatedWithCompanyCommand>
    {
        public async Task Handle(CreateModuleAssociatedWithCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _unitOfWork.Companies.FirstOrDefaultAsync(x => x.Id == request.CompanyId, cancellationToken);

            if (company is null)
            {
                throw new Exception("No existe una empresa asociado a este id!");
            }

            await _unitOfWork.Modules.CreateModuleAssociatedWithCompany(new()
            {
                ModuleName = request.ModuleName!,
                CompanyId = request.CompanyId
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}