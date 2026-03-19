using MediatR;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Authentication.v1.Handlers
{
    public class CreateNewUserHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IPasswordHasher _passwordHasher) : IRequestHandler<CreateNewUserCommand, CreateUserDto>
    {
        public async Task<CreateUserDto> Handle(CreateNewUserCommand request, CancellationToken cancellationToken)
        {
            User? user = null;

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

                if(user is not null)
                {
                    return _errorManager.ThrowBadRequest<CreateUserDto>("Ya existe un usuario con este correo asociado", "CreateUserByEmail");
                }
            }
            else if (!string.IsNullOrWhiteSpace(request.Username))
            {
                user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserName == request.Username, cancellationToken);

                if(user is not null)
                {
                    return _errorManager.ThrowBadRequest<CreateUserDto>("Ya existe un usuario con este nombre de usuario", "CreateUserByUsername");
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
            var passwordHash = _passwordHasher.HashPassword(request.Password!);
            var fullNameGenerated = request.FullName is null ? request.Username : request.FullName;

            //Creamos el usuario
            var newUser = new User()
            {
                UserName = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                Fullname = fullNameGenerated,
                UserStatus = UserStatus.Active,
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


            var modulesCode = request.ModulesWithAccess.Select(module => module.ModuleCode); 
            var roles = request.ModulesWithAccess.Select(role => role.RoleId);

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
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            //Modulos asignados con sus respectivo role.
            //Status 201 ✅
            return new ()
            {
                UserName = userCreated.UserName,
                Description = "Se creo el usuario de forma exitosa!",
                FullName = userCreated.Fullname
            };
        }
    }
}