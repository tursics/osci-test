using System;

namespace Osci.Exceptions
{
    /// <summary>
    /// Zusammenfassung für SAXException.
    /// </summary>
    public class SaxException 
        : Exception
    {
        public SaxException()
        {
        }
        public SaxException(Exception ex) 
            : base("", ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        public SaxException(string message) 
            : base(message)
        {
        }
        public SaxException(string message, Exception ex) 
            : base(message, ex)
        {
        }
    }
}
