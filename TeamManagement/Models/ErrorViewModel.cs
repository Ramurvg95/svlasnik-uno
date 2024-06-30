namespace TeamManagement.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string TeamNameErrorMessage { get; set; }

        public string TeamUsersErroMessage { get; set; }


    }
}
