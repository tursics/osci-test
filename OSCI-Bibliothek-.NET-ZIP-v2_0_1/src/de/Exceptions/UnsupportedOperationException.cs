using System;

namespace Osci.Exceptions 
{
    /// <summary>
    /// Zusammenfassung für UnsupportedOperationException.
    /// </summary>
    public class UnsupportedOperationException:Exception 
    {
        public UnsupportedOperationException(Exception ex)
            : base("",ex) 
        {
        }

        public UnsupportedOperationException()
        {
        }

        public UnsupportedOperationException(string message)
            : base(message)
        {
        }
    }
}
