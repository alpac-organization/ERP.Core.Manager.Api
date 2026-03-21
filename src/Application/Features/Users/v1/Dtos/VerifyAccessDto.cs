namespace ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos
{
    public class VerifyAccessDto
    {
        public bool HasAccess { get; set; }
        public string? Message { get; set; }
    }
}