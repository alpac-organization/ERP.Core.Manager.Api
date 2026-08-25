using ERP.Core.Database.Domain.Entities.Auth;

using Commands = ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands;


namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public static class DeviceMapper
    {
        public static Device ToDeviceEntity(this Commands.RegisterPushTokenCommand command, Guid profileId, string endpointArn)
        {
            return new()
            {
                Id                   = Guid.NewGuid(),
                IsActive             = true,
                UserProfileId        = profileId,
                DeviceName           = command.DeviceName,
                FcmToken             = command.Token,
                EndpointArn          = endpointArn
            };
        }
    }
}