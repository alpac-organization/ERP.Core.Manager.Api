using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;
using ERP.Core.Testing.Seeding;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands;
using System.Runtime.CompilerServices;

namespace ERP.Core.Manager.Api.IntegrationTests.Features.Authentication
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

        [Test]
        public async Task Login_CompanyWithoutProfileForUser_ReturnsBadRequest()
        {
            var user = await UnitOfWork.Users.FirstOrDefaultAsync(u => u.Id == DefaultUserId, CancellationToken.None);

            var randomCompany = Guid.NewGuid();
            var body = new { email = user!.Email, password = ErpSeedDataFactory.DefaultPassword };

            var response = await Client.PostAsJsonAsync($"/api/v1/companies/{randomCompany}/auth/login", body);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }

}