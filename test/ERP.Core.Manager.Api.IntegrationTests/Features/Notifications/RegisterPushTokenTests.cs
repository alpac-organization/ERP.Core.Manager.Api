using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands;

namespace ERP.Core.Manager.Api.IntegrationTests.Features.Notifications;

[TestFixture]
public class RegisterPushTokenTests : IntegrationTestBase
{
    [Test]
    [TestCase("AMINSA",  "Samsung Galaxy S24")]
    [TestCase("AVASA",   "Samsung Galaxy S24")]
    [TestCase("ALPAC",   "Samsung Galaxy S24")]
    [TestCase("VIGEMSA", "Samsung Galaxy S24")]
    [TestCase("TMN",     "Samsung Galaxy S24")]
    public async Task RegisterPushToken_HappyPath_PersistsDeviceInDatabase(string companyAlias, string deviceName)
    {
        var company = await UnitOfWork.Companies.Entities
            .Where(company => company.Alias == companyAlias)
            .FirstOrDefaultAsync(default);

        var token = $"fcm-{Guid.NewGuid()}";

        var command = new RegisterPushTokenCommand()
        {
            DeviceName = deviceName,
            Token      = token
        };

        var response = await SendAsync(HttpMethod.Post, $"/api/v1/companies/{company!.Id}/notifications/register-device-token", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var device = await UnitOfWork.Devices.Entities
            .FirstOrDefaultAsync(d => d.FcmToken == token);

        Assert.That(device, Is.Not.Null);
    }

}
