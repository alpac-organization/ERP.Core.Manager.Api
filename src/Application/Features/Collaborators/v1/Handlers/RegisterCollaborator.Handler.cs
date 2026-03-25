using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;
using ERP.Core.Manager.Api.Domain.Interfaces;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class RegisterCollaboratorHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager) : IRequestHandler<RegisterCollaboratorCommand>
    {

        public async Task Handle(RegisterCollaboratorCommand request, CancellationToken cancellationToken)
        {
            


            return;
        }
    }
}