using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries
{
    public class GetTypesAccountingPayrollQuery: BaseRequest, IRequest<List<TypesAccountingPayrollDto>>
    {
        
    }
}