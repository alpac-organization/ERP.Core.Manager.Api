using ERP.Core.Domain.Enums;
using ERP.Core.Domain.Entities.AWS;
using ERP.Core.Application.Commons.Interfaces.AWS;

namespace ERP.Core.Manager.Api.Tests.Common.Services;

/// <summary>
/// Sustituto de AWS SNS: evita llamadas de red reales durante las pruebas de integración.
/// El registro y envío de notificaciones se simulan localmente.
/// </summary>
public class FakeSimpleNotificationServices : ISimpleNotificationServices
{
    public const string FakeEndpointArn = "arn:aws:sns:us-east-1:000000000000:test-endpoint";

    public Task<string?> RegisterDeviceAsync(string fcmToken, string? customUserData = null)
        => Task.FromResult<string?>(FakeEndpointArn);

    public Task<bool> UnregisterDeviceAsync(string endpointArn)
        => Task.FromResult(true);

    public Task<PushSendResult> SendPushNotificationAsync(string endpointArn, NotificationRequest notificationRequest, Dictionary<string, string>? data = null)
        => Task.FromResult(PushSendResult.Sent);

    public Task<bool> SendToTopicAsync(string topicArn, string title, string body, Dictionary<string, string>? data = null)
        => Task.FromResult(true);
}
