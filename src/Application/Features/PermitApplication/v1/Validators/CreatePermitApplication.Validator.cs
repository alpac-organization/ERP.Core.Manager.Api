using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Validators
{
    public class CreatePermitApplicationValidator : AbstractValidator<CreatePermitApplicationCommand>
    {
        public CreatePermitApplicationValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                    .WithMessage("El id de la empresa no puedes vacio.")
                .NotNull()
                    .WithMessage("El id de la empresa es requerido");

            RuleFor(x => x.ModuleCode)
                .NotEmpty()
                    .WithMessage("El codigo de modulo es requerido")
                .NotNull()
                    .WithMessage("El codigo de modulo es requerido");

            RuleFor(x => x.UserId)
                .NotEmpty()
                    .WithMessage("El id de usuario es requerido")
                .NotNull()
                    .WithMessage("El id de usuario es requerido");

            RuleFor(x => x.IdentificationNumber)
                .NotEmpty()
                    .WithMessage("El número de identificación es requerido")
                .NotNull()
                    .WithMessage("El número de identificación es requerido");   

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("La descripción no puede exceder los 500 caracteres");

            RuleFor(x => x.PermitApplicationType)
                .NotNull()
                    .WithMessage("El tipo de permiso es requerido.")
                .IsInEnum()
                    .WithMessage("El tipo de permiso seleccionado no es válido.");

            When(x => x.PermitApplicationType == PermitApplicationType.Vacation, () => 
            {
                RuleFor(x => x.PermitApplicationVacation)
                    .NotNull()
                    .WithMessage("Los datos de la solicitud de vacaciones son obligatorios.");

                When(x => x.PermitApplicationVacation != null, () => 
                {
                    RuleFor(x => x.PermitApplicationVacation!.StartDate)
                        .NotEmpty()
                        .WithMessage("La fecha de inicio es requerida.");

                    RuleFor(x => x.PermitApplicationVacation!.EndDate)
                        .NotEmpty()
                        .WithMessage("La fecha de fin es requerida.")
                        .Must((command, endDate) => endDate.Date >= command.PermitApplicationVacation!.StartDate.Date)
                        .WithMessage("La fecha de fin no puede ser menor a la de inicio.");

                    RuleFor(x => x)
                        .Must(x => {
                            var days = (x.PermitApplicationVacation!.EndDate.Date - x.PermitApplicationVacation.StartDate.Date).Days + 1;
                            return days <= 30;
                        })
                        .WithMessage("No puedes solicitar más de 30 días de vacaciones.");
                });
            });


            When(x => x.PermitApplicationType == PermitApplicationType.DonatedVacations, () =>
            {
                
            });

        }
    }
}