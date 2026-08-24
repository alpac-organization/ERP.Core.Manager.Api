namespace ERP.Core.Manager.Api.Application.Commons.Options
{
    public class NotificationsOptions
    {
        public const string SectionName = "PushNotifications";

        public string Provider { get; set; } = string.Empty;
        public string ServerKey { get; set; } = string.Empty;
        public NotificationCopy DeviceRegistrationCopies { get; set; } = new();
        public RequisitionCopiesOptions RequisitionCopies { get; set; } = new();
    }

    public class RequisitionCopiesOptions
    {
        public NotificationCopy Created { get; set; } = new();
        public NotificationCopy PendingApproval { get; set; } = new();
        public NotificationCopy Approved { get; set; } = new();
        public NotificationCopy Rejected { get; set; } = new();
    }

    public class NotificationCopy
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Sound { get; set; } = "default";
    }
}