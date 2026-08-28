using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;

using ERP.Core.Manager.Api.Tests.Common;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands;

namespace ERP.Core.Manager.Api.Tests.Controllers.Authentication
{
    [TestFixture]
    public class LoginTests : IntegrationTestBase
    {
        [Test]
        [TestCase("ALPAC")]
        [TestCase("ALPAC")]
        [TestCase("AMINSA")]
        [TestCase("VIGEMSA")]
        [TestCase("ALPAC")]
        [TestCase("ALPAC")]
        public async Task Login_WithValidCredentials_ReturnsAccessAndRefreshToken(string companyAlias)
        {
            var user = await UnitOfWork.Users.Entities
                .Where(user => user.UserStatus == UserStatus.Active)
                .FirstOrDefaultAsync(u => u.Id == DefaultUserId);

            //Crear commmand
            var command = new LoginWithUsernameAndPasswordCommand (){ 
                Username = user?.UserName ?? "", 
                Password = ErpSeedDataFactory.DefaultPassword 
            };

            var company = await UnitOfWork.Companies.Entities
                .Where(company => company.Alias == companyAlias)
                .FirstOrDefaultAsync(default);

            var response = await SendAsync(HttpMethod.Post,$"/api/v1/companies/{company?.Id}/auth/login", command);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json  = await response.Content.ReadAsStringAsync();
            var login = JsonSerializer.Deserialize<LoginDto>(json, SnakeCaseJsonOptions());

            Assert.Multiple(() =>
            {
                Assert.That(login, Is.Not.Null);
                Assert.That(login!.AccessToken, Is.Not.Null.And.Not.Empty);
                Assert.That(login.RefreshToken, Is.Not.Null.And.Not.Empty);
            });
        }
        
        [Test]
        [TestCase("ALPAC")]
        [TestCase("ALPAC")]
        [TestCase("AMINSA")]
        [TestCase("VIGEMSA")]
        [TestCase("ALPAC")]
        [TestCase("ALPAC")]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized(string companyAlias)
        {
            var user = await UnitOfWork.Users.Entities
                .Where(user => user.Id == DefaultUserId)
                .FirstOrDefaultAsync(default);

            //Crear commmand
            var command = new LoginWithUsernameAndPasswordCommand (){ 
                Username = user?.UserName ?? "", 
                Password = "InvalidPassword" 
            };

            var company = await UnitOfWork.Companies.Entities
               .Where(company => company.Alias == companyAlias)
               .FirstOrDefaultAsync(default);

            var response = await SendAsync(HttpMethod.Post, $"/api/v1/companies/{company?.Id}/auth/login", command);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }
}