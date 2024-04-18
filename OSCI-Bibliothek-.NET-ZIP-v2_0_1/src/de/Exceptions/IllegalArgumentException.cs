using System;

namespace Osci.Exceptions
{
    /// <summary>
    /// Zusammenfassung für IllegalArgumentException.
    /// </summary>
    public class IllegalArgumentException
        : Exception
    {
        public IllegalArgumentException(Exception ex)
            : base("", ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }

        public IllegalArgumentException(string message)
            : base(message)
        {
        }
        public IllegalArgumentException()
        {
        }
        public IllegalArgumentException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}
