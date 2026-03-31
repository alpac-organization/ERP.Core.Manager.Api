using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Users.v1.Handlers
{
    public class GetAvailableModulesForUserHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager) : UserValidateHandlerBase(_unitOfWork, _errorManager), IRequestHandler<GetAvailableModulesForUserQuery, List<UserModuleDto>>
    {
        public async Task<List<UserModuleDto>> Handle(GetAvailableModulesForUserQuery request, CancellationToken cancellationToken)
        {
            var profile = await ValidateUserAndProfileAsync(request.UserId, request.CompanyId, cancellationToken);

            var userRoles = await _unitOfWork.UserModules.Entities
                .Include(ur => ur.Role)
                .Where(ur => ur.UserProfileId == profile.Id && ur.IsActive)
                .ToListAsync(cancellationToken);

            if (userRoles.Count == 0) return [];

            var moduleCodes = userRoles
                .Select(ur => ur.ModuleCode)
                .Where(code => code != null)
                .Distinct()
                .ToList();

            var masterModules = await _unitOfWork.Modules.Entities
                .Where(m => moduleCodes.Contains(m.Code))
                .ToDictionaryAsync(
                    m => m.Code!, 
                    m => (Name: m.ModuleName, Desc: m.Description, Path: m.PathRedirect),
                    cancellationToken
                );

            var result = userRoles.Select(ur => 
            {
                if (ur.ModuleCode != null && masterModules.TryGetValue(ur.ModuleCode, out var info))
                {
                    return new UserModuleDto
                    {
                        ModuleCode = ur.ModuleCode,
                        RoleType = ur.Role?.RoleType.ToString(),
                        ModuleName = info.Name,
                        Description = info.Desc,
                        PathRedirect = info.Path
                    };
                }

                return new UserModuleDto
                {
                    ModuleCode = ur.ModuleCode,
                    RoleType = ur.Role?.RoleType.ToString(),
                    ModuleName = "Módulo no encontrado",
                    Description = "Sin descripción"
                };
            }).ToList();

            return result;
        }
    }
}