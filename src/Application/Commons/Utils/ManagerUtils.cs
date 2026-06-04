using ERP.Core.Database.Domain.Entities.Payrolls;

namespace ERP.Core.Manager.Api.Application.Commons.Utils
{
    public static class ManagerUtils
    {
        public static string FromSliceToCollaboratorFullname(Collaborator collaborator)
        {
            var fullNames = new[] 
            { 
                collaborator.FirstName, 
                collaborator.SecondName, 
                collaborator.ThirdName,
                collaborator.FirstLastname, 
                collaborator.SecondLastname 
            };

            return string.Join(" ", fullNames.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n?.Trim()));
        }

    }
}
