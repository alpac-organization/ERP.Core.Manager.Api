using MediatR;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Handlers
{
    public class LoginWithUsernameAndPasswordHandler(IUnitOfWork _unitOfWork) : IRequestHandler<LoginWithUsernameAndPasswordCommand, LoginDto>
    {
        
        public async Task<LoginDto> Handle(LoginWithUsernameAndPasswordCommand request, CancellationToken cancellationToken)
        {

            await _unitOfWork.Companies.GetAvailableCompanies(cancellationToken);

            var response = new LoginDto()
            {
                  
            };

            return new ();
        }
    }
}