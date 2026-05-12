using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Loans.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Loans.v1.Handlers
{
    public class RegisterLoanHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager) : AlpacBaseHandler<RegisterLoanCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(RegisterLoanCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(
                request.UserId,
                request.CompanyId,
                request.ModuleCode!,
                cancellationToken);

            if (!access.IsSuccess)
                return access.ErrorResponse!;

            decimal exchangeRate = 36.6243m;

            decimal totalAmountCordobas;
            decimal totalAmountDollars;

            if (request.Currency == Currency.USD)
            {
                totalAmountDollars = request.Amount;
                totalAmountCordobas = request.Amount * exchangeRate;
            }
            else
            {
                totalAmountCordobas = request.Amount;
                totalAmountDollars = request.Amount / exchangeRate;
            }

            // Cuota quincenal
            decimal fortnightlyCordobas =
                totalAmountCordobas / request.NumberFortnights;

            decimal fortnightlyDollars =
                totalAmountDollars / request.NumberFortnights;

            // Aplicar primera cuota al registrar
            decimal amountPaidCordobas = fortnightlyCordobas;
            decimal amountPaidDollars = fortnightlyDollars;

            decimal balanceCordobas =
                totalAmountCordobas - amountPaidCordobas;

            decimal balanceDollars =
                totalAmountDollars - amountPaidDollars;

            int remainingFortnights =
                request.NumberFortnights;


            await _unitOfWork.Deductions.RegisterDeduction(new()
            {
                Type = DeductionType.Loans,
                Currency = request.Currency,
                CollaboratorId = request.CollaboratorId,
                Description = request.Description ?? "Sin descripción",
                Status = DeductionStatus.Progress,

                FortnightlyAmount = fortnightlyCordobas,
                FortnightlyAmountInDollars = fortnightlyDollars,

                AmountPaid = amountPaidCordobas,
                AmountPaidInDollars = amountPaidDollars,

                NumberOfFortnights = remainingFortnights,

                TotalAmount = totalAmountCordobas,
                TotalAmountInDollars = totalAmountDollars,

                TotalBalance = balanceCordobas,
                TotalBalanceInDollars = balanceDollars
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}