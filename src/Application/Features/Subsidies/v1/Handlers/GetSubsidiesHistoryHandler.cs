using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Dtos;
using Microsoft.Extensions.Logging;

namespace ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Handlers
{
    public class GetSubsidiesHistoryHandler(
        IUnitOfWork _unitOfWork,
        IErrorManager _errorManager,
        ILogger<GetSubsidiesHistoryHandler> _logger)
        : AlpacBaseHandler<GetSubsidiesHistoryQuery, PagedResponse<SubsidyHistoryDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<PagedResponse<SubsidyHistoryDto>> Handle(
            GetSubsidiesHistoryQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Consultando historial de subsidios para la compañía: {CompanyId}", request.CompanyId);

            var access = await ValidateAccessAsync(
                request.UserId,
                request.CompanyId,
                request.ModuleCode,
                cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            var query = _unitOfWork.Subsidies.Entities
                .AsNoTracking()
                .Include(s => s.TypesSubsidy)
                .Include(s => s.Collaborator)
                    .ThenInclude(c => c.WorkingInformation)
                .Include(s => s.Collaborator)
                    .ThenInclude(c => c.Salaries.Where(sal => sal.EndDate == null))
                .Where(s => s.Collaborator.CompanyId == request.CompanyId);

            if (request.BranchId != Guid.Empty)
            {
                query = query.Where(s =>
                    s.Collaborator.WorkingInformation.CompanyBranchId == request.BranchId);
            }

            if (!string.IsNullOrEmpty(request.IdentificationNumber))
            {
                query = query.Where(s =>
                    s.Collaborator.IdentificationNumber == request.IdentificationNumber);
            }

            if (request.AreaId.HasValue)
            {
                query = query.Where(s =>
                    s.Collaborator.WorkingInformation.AreaId == request.AreaId);
            }

            var totalRecords = await query.CountAsync(cancellationToken);

            var subsidies = await query
                .OrderByDescending(s => s.StartDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var mappedRecords = subsidies.Select(s =>
            {
                var currentMonthlySalary = s.Collaborator?.Salaries?.FirstOrDefault()?.AmountInLocal ?? 0;
                var dailySalary = currentMonthlySalary / 30;
                var totalSubsidyBaseAmount = dailySalary * s.AmountDays;
                var companyPercentage = s.Percentage / 100m;
                var inssPercentage = 1m - companyPercentage;

                return new SubsidyHistoryDto
                {
                    CollaboratorCode = s.Collaborator?.CollaboratorCode,
                    CollaboratorFullName = $"{s.Collaborator?.FirstName} {s.Collaborator?.FirstLastname}",
                    AmountDays = s.AmountDays,
                    ReferenceNumber = s.ReferenceNumber,
                    TypeSubsidyName = s.TypesSubsidy?.SubsidyName,
                    StartDate = DateOnly.FromDateTime(s.StartDate),
                    EndDate = DateOnly.FromDateTime(s.EndDate),
                    Percentage = s.Percentage,
                    CompanyAssumedAmount = Math.Round(totalSubsidyBaseAmount * companyPercentage, 2),
                    InssReimbursementAmount = Math.Round(totalSubsidyBaseAmount * inssPercentage, 2)
                };
            }).ToList();

            return new PagedResponse<SubsidyHistoryDto>(
                mappedRecords,
                request.PageNumber,
                request.PageSize,
                totalRecords
            );
        }
    }
}