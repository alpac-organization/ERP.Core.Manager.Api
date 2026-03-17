namespace ERP.Core.Manager.Api.Domain.Entities.Errors
{
    public class ErrorResponse
    {
        public int Status { get; set; }
        public ErrorDetails Error { get; set; }
        public string CreatedAt { get; set; }

        public ErrorResponse(int status, string type, string description)
        {
            Status = status;
            Error = new ErrorDetails { TypeError = type, Description = description };
            CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    public class ErrorDetails
    {
        public string TypeError { get; set; } = "UnknowError";
        public string Description { get; set; } = "UnknowDescription";
    }
}