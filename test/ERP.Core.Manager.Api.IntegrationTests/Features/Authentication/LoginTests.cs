using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;
using ERP.Core.Testing.Seeding;

namespace ERP.Core.Manager.Api.IntegrationTests.Features.Authentication;

/// <summary>
/// Pruebas de integración del flujo de autenticación (login) usando los usuarios reales
/// sembrados por <see cref="ErpSeedDataFactory"/> (misma contraseña para todos: Admin123!).
/// </summary>
[TestFixture]
public class LoginTests : IntegrationTestBase
{
    [Test]
    public async Task Login_WithValidCredentials_ReturnsAccessAndRefreshToken()
    {
        var user = await UnitOfWork.Users.FirstOrDefaultAsync(u => u.Id == DefaultUserId, CancellationToken.None);
        Assert.That(user, Is.Not.Null, "Debe existir el usuario sembrado por defecto.");

        var body = new { email = user!.Email, password = ErpSeedDataFactory.DefaultPassword };

        var response = await Client.PostAsJsonAsync($"/api/v1/companies/{DefaultCompanyId}/auth/login", body);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = await response.Content.ReadAsStringAsync();
        var login = JsonSerializer.Deserialize<LoginDto>(json, SnakeCaseJsonOptions());

        Assert.That(login, Is.Not.Null);
        Assert.That(login!.AccessToken, Is.Not.Null.And.Not.Empty);
        Assert.That(login.RefreshToken, Is.Not.Null.And.Not.Empty);
        Assert.That(login.UserId, Is.EqualTo(DefaultUserId));
        Assert.That(login.CompanyInformation.CompanyId, Is.EqualTo(DefaultCompanyId));
    }

    [Test]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var user = await UnitOfWork.Users.FirstOrDefaultAsync(u => u.Id == DefaultUserId, CancellationToken.None);

        var body = new { email = user!.Email, password = "WrongPassword1!" };

        var response = await Client.PostAsJsonAsync($"/api/v1/companies/{DefaultCompanyId}/auth/login", body);

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
