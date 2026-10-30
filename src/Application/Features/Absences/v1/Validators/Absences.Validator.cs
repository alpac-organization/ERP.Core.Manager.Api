using FluentValidation;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Validators
{
    public class RegisterAbsenceValidator : BaseRequestValidator<RegisterAbsenceCommand>
    {
        public RegisterAbsenceValidator()
        {
            RuleFor(x => x.CollaboratorId)
                .NotEmpty().WithMessage("El id del colaborador es requerido.")
                .NotEqual(Guid.Empty).WithMessage("El id del colaborador no es válido.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("La fecha de inasistencia es requerida.");

            RuleFor(x => x.Reason)
                .MaximumLength(500).WithMessage("El motivo no puede exceder los 500 caracteres.");
        }
    }

    public class ApplyAbsenceValidator : BaseRequestValidator<ApplyAbsenceCommand>
    {
        public ApplyAbsenceValidator()
        {
            RuleFor(x => x.AbsenceId)
                .NotEmpty().WithMessage("El id de la inasistencia es requerido.")
                .NotEqual(Guid.Empty).WithMessage("El id de la inasistencia no es válido.");
        }
    }

    public class CancelAbsenceValidator : BaseRequestValidator<CancelAbsenceCommand>
    {
        public CancelAbsenceValidator()
        {
            RuleFor(x => x.AbsenceId)
                .NotEmpty().WithMessage("El id de la inasistencia es requerido.")
                .NotEqual(Guid.Empty).WithMessage("El id de la inasistencia no es válido.");

            RuleFor(x => x.CancellationReason)
                .NotEmpty().WithMessage("El motivo de cancelación es obligatorio.")
                .MaximumLength(500).WithMessage("El motivo no puede exceder los 500 caracteres.");
        }
    }

    public class DeleteAbsenceValidator : BaseRequestValidator<DeleteAbsenceCommand>
    {
        public DeleteAbsenceValidator()
        {
            RuleFor(x => x.AbsenceId)
                .NotEmpty().WithMessage("El id de la inasistencia es requerido.")
                .NotEqual(Guid.Empty).WithMessage("El id de la inasistencia no es válido.");
        }
    }

    public class GetAbsencesValidator : BaseRequestValidator<GetAbsencesQuery>
    {
        public GetAbsencesValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("El número de página debe ser mayor a 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("El tamaño de página debe ser mayor a 0.")
                .LessThanOrEqualTo(100).WithMessage("El tamaño de página no puede exceder 100.");
        }
    }

    public class GetAbsenceByIdValidator : BaseRequestValidator<GetAbsenceByIdQuery>
    {
        public GetAbsenceByIdValidator()
        {
            RuleFor(x => x.AbsenceId)
                .NotEmpty().WithMessage("El id de la inasistencia es requerido.")
                .NotEqual(Guid.Empty).WithMessage("El id de la inasistencia no es válido.");
        }
    }
}
