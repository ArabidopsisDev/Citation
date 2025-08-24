namespace Citation.Model
{
    public class FailedMessage(FailedMessage.FailedReason reason, string message)
    {
        public FailedReason Reason { get; set; } = reason;
        public string Message { get; set; } = message;

        public enum FailedReason
        {
            Success,
            Expired,
            InvalidSignature,
            Unauthorized,
            Exception
        }
    }
}
