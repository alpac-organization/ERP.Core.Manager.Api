using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos
{
    public class DocumentDto: BaseDocumentData
    {
        public string? CollaboratorFullname { get; set; }
        public string? JobPositionName { get; set; }
        public string? EntryDate { get; set; }
        public string? CurrentSalary { get; set; }
        public string? SalaryInLetters { get; set; }
    }
}