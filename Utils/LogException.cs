namespace Citation.Utils
{
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
            Acceed.Shared.FeBr(insertString);
        }
    }
}
