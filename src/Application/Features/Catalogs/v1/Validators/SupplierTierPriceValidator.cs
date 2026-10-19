
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using FluentValidation;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators
{
public class TierPriceValidator : AbstractValidator<TierPriceDto>
{
    public TierPriceValidator(
        bool requireValidFrom = false,
        bool requireValidFromNotInPast = false,
        bool requireCoreFields = false)
     {
        if (requireCoreFields)
        {
            RuleFor(t => t.MinQuantity)
                .NotNull()
                .WithMessage("La cantidad mínima es obligatoria.");

            RuleFor(t => t.PreferentialPrice)
                .NotNull()
                .WithMessage("El precio preferencial es obligatorio.");

            RuleFor(t => t.ValidFrom)
                .NotNull()
                .WithMessage("La fecha de inicio de vigencia es obligatoria.");
        }

        RuleFor(t => t.MinQuantity)
            .GreaterThan(0)
            .WithMessage("La cantidad mínima debe ser mayor a cero.")
            .When(t => t.MinQuantity.HasValue);

        RuleFor(t => t.PreferentialPrice)
             .GreaterThanOrEqualTo(0)
             .WithMessage("El precio preferencial no puede ser negativo.")
             .When(t => t.PreferentialPrice.HasValue);

        if (requireValidFrom)
        RuleFor(t => t.ValidFrom)
             .NotNull()
             .WithMessage("La fecha de inicio de vigencia es obligatoria.");

        if (requireValidFromNotInPast)
        RuleFor(t => t.ValidFrom)
            .Must(from => from >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha inicial del precio preferencial no puede ser anterior al día actual.")
            .When(t => t.ValidFrom.HasValue);

        RuleFor(t => t.UnitMeasureId)
            .NotEmpty()
            .WithMessage("El id de la unidad de medida no es válido.")
            .When(t => t.UnitMeasureId.HasValue);

        RuleFor(t => t.Currency)
            .IsInEnum()
            .WithMessage("La moneda del precio preferencial no es válida.")
            .When(t => t.Currency.HasValue);
        
        RuleFor(t => t)
            .Must(t => t.ValidTo!.Value >= t.ValidFrom!.Value)
            .WithMessage("La fecha de fin de vigencia no puede ser anterior a la fecha de inicio.")
            .When(t => t.ValidTo.HasValue && t.ValidFrom.HasValue);
     }
   }
}