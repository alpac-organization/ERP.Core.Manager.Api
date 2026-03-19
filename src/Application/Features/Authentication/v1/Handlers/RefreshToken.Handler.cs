using MediatR;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Handlers
{
    public class RefreshTokenHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IAuthServices _authServices) : IRequestHandler<RefreshTokenCommand, LoginDto>
    {
        public async Task<LoginDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
          

            return new LoginDto
            {
                
            };
        }
    }
}