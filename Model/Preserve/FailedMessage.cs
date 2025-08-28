namespace Citation.Model.Preserve;

public class FailedMessage(FailedMessage.FailedReason reason, string message, string password = "")
{
    public FailedReason Reason { get; set; } = reason;
    public string Message { get; set; } = message;

    public string Password { get; set; } = password;

    public enum FailedReason
    {
        Success,
        Expired,
        InvalidSignature,
        Unauthorized,
        Exception
    }
}
