using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries;
using ERP.Core.Manager.Api.Domain.Enums;
using System.Globalization;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class GenerateDocumentToCollaboratorHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IPdfGeneratorServices pdfGeneratorServices): AlpacBaseHandler<GenerateDocumentToCollaboratorQuery, byte[]>(_unitOfWork, _errorManager)
    {
        public override async Task<byte[]> Handle(GenerateDocumentToCollaboratorQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            var collaboratorInformation = await _unitOfWork.Collaborators.Entities
                .Include(col => col.Company)
                .Include(col => col.WorkingInformation)
                    .ThenInclude(work => work.WorkPosition)
                .Where(col => col.IdentificationNumber == request.IdentificationNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (collaboratorInformation is null)
            {
                return _errorManager.ThrowBadRequest<byte[]>("Este colaborador no existe en nuestro sistema", "ERP;:01");
            }

            var fullName = string.Join(" ", new[] 
            { 
                collaboratorInformation.FirstName, 
                collaboratorInformation.SecondName, 
                collaboratorInformation.ThirdName, 
                collaboratorInformation.FirstLastname, 
                collaboratorInformation.SecondLastname 
            }.Where(s => !string.IsNullOrWhiteSpace(s)));

            var now = DateTime.Now;
            var culture = new CultureInfo("es-NI");


            var payload = new DocumentDto()
            {
                CollaboratorFullname = fullName,
                CompanyImageUrl = collaboratorInformation.Company.ImageUrl,  
                CompanyName = collaboratorInformation.Company.CompanieName,
                JobPositionName = collaboratorInformation.WorkingInformation.WorkPosition.CatalogName,
                EntryDate = collaboratorInformation.WorkingInformation.EntryDate.ToString("dd 'de' MMMM 'de' yyyy", culture),
                CurrentDay = now.Day.ToString(),
                CurrentMonthName = now.ToString("MMMM", culture),
                CurrentYear = now.Year.ToString()
            };


            switch (request.DocumentType)
            {
                case DocumentType.LetterCollaboratorActive :
                {
                    return await pdfGeneratorServices.GenerateAsync<DocumentDto>("LetterCollaboratorActive", payload);
                }
                case DocumentType.SalaryLetter :
                {

                    var salaryInfo = await _unitOfWork.Salaries.Entities
                        .Where(salary => salary.CollaboratorId == collaboratorInformation.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (salaryInfo is null)
                    {
                        return _errorManager.ThrowBadRequest<byte[]>("Ocurrio un error al consultar la información salarial. Consulte con el departamento de IT", "erp:001");
                    }

                    payload.CurrentSalary = salaryInfo.AmountInLocal.ToString("N2", culture);
                    payload.SalaryInLetters = StringExtensions.ToNumberToLetters(salaryInfo.AmountInLocal);

                    return await pdfGeneratorServices.GenerateAsync<DocumentDto>("SalaryLetter", payload);   
                }
                default:
                {
                    return _errorManager.ThrowBadRequest<byte[]>("Este tipo de documento no esta disponible", "ERP:02");
                }
            }
        }
    }
}