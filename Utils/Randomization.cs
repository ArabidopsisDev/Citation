using System.Text;

namespace Citation.Utils
{
    internal class Randomization
    {
        internal static string RandomSeries()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            const int length = 16;

            var random = new Random();
            var builder = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                builder.Append(chars[random.Next(chars.Length)]);
            }

            return builder.ToString();
        }
    }
}
