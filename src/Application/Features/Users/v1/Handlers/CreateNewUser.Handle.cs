using MediatR;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Users.v1.Handlers
{
    public class CreateNewUserHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IPasswordHasher _passwordHasher, ICodeGenerator _codeGenerator) : IRequestHandler<CreateNewUserCommand, CreateUserDto>
    {
        public async Task<CreateUserDto> Handle(CreateNewUserCommand request, CancellationToken cancellationToken)
        {
            User? user = null;

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

                if(user is not null)
                {
                    return _errorManager.ThrowBadRequest<CreateUserDto>("Ya existe un usuario con este correo asociado", "ERP:01");
                }
            }

            var modulesWithAccess = request.ModulesWithAccess;

            if (request.ModulesWithAccess == null || request.ModulesWithAccess.Count == 0)
            {
                return _errorManager.ThrowBadRequest<CreateUserDto>(
                    "El usuario debe tener al menos un módulo de acceso asignado", 
                    "CreateUserNoModules"
                );
            }

            //Hashiamos la contraseña
            var username = _codeGenerator.GenerateUsername(request.FullName!);
            var passwordHash = _passwordHasher.HashPassword(request.Password!);

            //Creamos el usuario
            var newUser = new User()
            {
                UserName = username,
                Email = request.Email,
                PasswordHash = passwordHash,
                Fullname = request.FullName,
                UserStatus = UserStatus.Active,
                UserType = request.UserType
            };

            var userCreated = await _unitOfWork.Users.CreateNewUser(newUser);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //Usuario Creado con exito
            var company = await _unitOfWork.Companies.FirstOrDefaultAsync(company => company.Id == request.CompanyId, cancellationToken);

            if (company is null)
            {
                return _errorManager.ThrowBadRequest<CreateUserDto>("La empresa seleccionada no existe", "CompanyNotFound");
            }

            var userProfile = new UserProfile()
            {
                CompanyId = request.CompanyId,
                IsActive = true,
                UserId = userCreated.Id
            };

            //Creamos su perfil y lo asociamos a la empresa.
            var userProfileCreated = await _unitOfWork.Profiles.CreateNewUserProfile(userProfile);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var roles = request.ModulesWithAccess.Select(role => role.RoleId);
            var modulesCode = request.ModulesWithAccess.Select(module => module.ModuleCode); 

            foreach(var module in request.ModulesWithAccess)
            {
                var moduleExist = await _unitOfWork.Modules
                    .FirstOrDefaultAsync(m => m.Code == module.ModuleCode && m.CompanyId == userProfileCreated.CompanyId, cancellationToken);

                if (moduleExist is null)
                {
                    return _errorManager.ThrowBadRequest<CreateUserDto>("Este modulo no existe en el sistema", "ERP:ModuleNotFound");
                }

                var roleIsValid = await _unitOfWork.Roles.FirstOrDefaultAsync(r => r.Id == module.RoleId, cancellationToken);

                if (roleIsValid is null)
                {
                    return _errorManager.ThrowBadRequest<CreateUserDto>("Este role no es valido.", "ERP:RoleInvalid");
                }

                await _unitOfWork.UserModules.AssignRolesModule(module.RoleId, module.ModuleCode!, userProfileCreated.Id);
            }

            //Guardar Cambios en la base de datos
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ()
            {
                UserName = userCreated.UserName,
                FullName = userCreated.Fullname,
                UserType = userCreated.UserType,
                Description = "Usuario Creado Con exito!"
            };
        }
    }
}