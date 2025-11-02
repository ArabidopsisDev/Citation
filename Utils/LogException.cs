namespace Citation.Utils;

/// <summary>
/// Provides functionality for collecting and logging exception information with an associated severity level.
/// </summary>
/// <remarks>This class is intended for internal use to record exception details, including the exception type,
/// message, and severity, for diagnostic or auditing purposes. It is not intended for use outside of the logging or
/// monitoring infrastructure.</remarks>
internal class LogException
{
    internal enum ExceptionLevel
    {
        Info,
        Warning,
        Error
    }

    internal static void Collect(Exception exception, ExceptionLevel level)
    {
        Console.WriteLine(exception.Message);

        var type = exception.GetType();
        var exceptionType = type.FullName;
        var exceptionMsg = exception.Message;
        var exceptionLevel = level.ToString();

        var insertString = $"""
                        INSERT into tb_Exception (ExceptionType, ExceptionLevel, ExceptionMsg)
                        VALUES ('{exceptionType}', '{exceptionLevel}', '{exceptionMsg}')
                        """;
        Acceed.Shared.Execute(insertString);
    }
}
