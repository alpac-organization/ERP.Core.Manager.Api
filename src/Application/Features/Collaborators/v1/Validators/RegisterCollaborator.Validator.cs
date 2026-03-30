using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Validators
{
    public class RegisterCollaboratorValidator : AbstractValidator<RegisterCollaboratorCommand>
    {
        public RegisterCollaboratorValidator()
        {

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El id de usario es requerido.")
                .NotNull().WithMessage("El id de usuario es requerido.");

            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("El id de la empresa es requerido.")
                .NotNull().WithMessage("El id de la empresa es requerido.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El primer nombre es obligatorio.")
                .NotNull().WithMessage("El primer nombre es obligatorio.");

            RuleFor(x => x.IdentificationNumber)
                .NotEmpty().WithMessage("El número de identificación es requerido.")
                .NotNull().WithMessage("El número de identificación es requerido");
            
            RuleFor(x => x.IdentificationType)
                .NotEmpty().WithMessage("El tipo de identificación debe ser especificado")
                .NotNull().WithMessage("El tipo de identificación debe ser especificado");

            RuleFor(x => x.FirstLastname)
                .NotEmpty().WithMessage("El primer apellido es obligatorio")
                .NotNull().WithMessage("El primer apellido es obligatorio");

            RuleFor(x => x.WorkingInformation)
                .NotNull().WithMessage("El bloque de información laboral no puede estar vacío.");

            RuleFor(x => x.SalaryInformation)
                .NotNull().WithMessage("El bloque de información salarial es obligatorio.");

            RuleFor(x => x.PersonalInformation)
                .SetValidator(new PersonalInformationValidator()!);

            RuleFor(x => x.WorkingInformation)
                .SetValidator(new WorkingInformationValidator()!)
                .When(x => x.WorkingInformation != null);

            RuleFor(x => x.SalaryInformation)
                .SetValidator(new SalaryInformationValidator()!)
                .When(x => x.SalaryInformation != null);
        }
    }

    public class PersonalInformationValidator : AbstractValidator<PersonalInformation>
    {
        public PersonalInformationValidator()
        {

            RuleFor(x => x.PersonalEmail)
                .EmailAddress()
                .WithMessage("El formato del correo electrónico no es válido")
                .When(x => !string.IsNullOrEmpty(x.PersonalEmail));

            RuleFor(x => x.PersonalPhoneNumber)
                .NotEmpty()
                .WithMessage("El número de teléfono de trabajo es obligatorio")
                .Matches(@"^[2|5|7|8]\d{7}$")
                .WithMessage("El número de teléfono debe ser válido para Nicaragua (8 dígitos y empezar con 2, 5, 7 u 8)")
                .When(x => !string.IsNullOrEmpty(x.PersonalPhoneNumber));
            
        }
    }

    public class WorkingInformationValidator : AbstractValidator<WorkingInformation>
    {
        public WorkingInformationValidator()
        {

            RuleFor(x => x.EntryDate)
                .NotEmpty() 
                .WithMessage("La fecha de ingreso es obligatoria")
                .NotEqual(default(DateTime))
                .WithMessage("La fecha de ingreso no es válida");

            RuleFor(x => x.BranchId) 
                .GreaterThan(0)
                .NotEmpty()
                .WithMessage("La sucural de trabajo es obligatoria");

            RuleFor(x => x.WorkEmail)
                .EmailAddress()
                .WithMessage("El formato del correo electrónico no es válido")
                .When(x => !string.IsNullOrEmpty(x.WorkEmail));

            RuleFor(x => x.WorkPhoneNumber)
                .NotEmpty()
                .WithMessage("El número de teléfono de trabajo es obligatorio")
                .Matches(@"^[2|5|7|8]\d{7}$")
                .WithMessage("El número de teléfono debe ser válido para Nicaragua (8 dígitos y empezar con 2, 5, 7 u 8)")
                .When(x => !string.IsNullOrEmpty(x.WorkPhoneNumber));

            RuleFor(x => x.WorkAreaId) 
                .GreaterThan(0)
                .NotEmpty()
                .WithMessage("El area de trabajo es obligatoria");

            RuleFor(x => x.WorkPositionId) 
                .GreaterThan(0)
                .NotEmpty()
                .WithMessage("La posición de trabajo es obligatoria");
        }
    }

    public class SalaryInformationValidator : AbstractValidator<SalaryInformation>
    {
        public SalaryInformationValidator()
        {
            RuleFor(x => x.Salary)
                .GreaterThan(0)
                .WithMessage("El salario es obligatorio.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("El tipo de moneda es obligatorio.");

            RuleFor(x => x.SalaryType)
                .NotEmpty()
                .WithMessage("El tipo de salario es obligatorio.");

            RuleFor(x => x.SubCatalogBankId)
                .GreaterThan(0)
                .NotEmpty()
                .WithMessage("El banco es obligatorio");
        }
    }
}