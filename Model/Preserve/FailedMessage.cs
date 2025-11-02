namespace Citation.Model.Preserve;

/// <summary>
/// Represents the result of a failed operation, including the reason for failure, an associated message, and an
/// optional password.
/// </summary>
/// <param name="reason">The reason the operation failed. Specifies the failure category.</param>
/// <param name="message">A descriptive message providing details about the failure.</param>
/// <param name="password">An optional password associated with the failed operation. Defaults to an empty string if not specified.</param>
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
