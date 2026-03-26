using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Users.v1.Validators
{
    public class GetAllActiveUsersByCompanyIdValidator : AbstractValidator<GetAllActiveUsersByCompanyIdQuery>
    {
        public GetAllActiveUsersByCompanyIdValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El ID de la empresa es obligatorio.");
        }
    }
}