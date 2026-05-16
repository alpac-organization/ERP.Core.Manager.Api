namespace ERP.Core.Manager.Api.Application.Features.Branches.v1.Dtos
{
    public class BranchesDto
    {
        public Guid BranchId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? BranchName { get; set; }

        public string? CompanyAlias { get; set; }
        public string? BranchAddress { get; set; }
        public Guid CompanyId { get; set; }
    }
}