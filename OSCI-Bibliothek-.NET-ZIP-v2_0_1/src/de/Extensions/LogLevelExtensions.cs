using Osci.Helper;

namespace Osci.Extensions
{
    internal static class LogLevelExtensions
    {
        public static string FriendlyName(this LogLevel level)
        {
            return level.ToString().ToUpperInvariant();
        }
    }
}