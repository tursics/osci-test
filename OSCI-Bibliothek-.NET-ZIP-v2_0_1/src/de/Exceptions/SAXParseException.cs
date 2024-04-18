using System;

namespace Osci.Exceptions
{
    /// <summary>
    /// Zusammenfassung für SAXParseException.
    /// </summary>
    public class SaxParseException
        : Exception
    {
        public SaxParseException()
        {
        }
        public SaxParseException(Exception ex) 
            : base("", ex)
        {
        }
        public SaxParseException(string message) 
            : base(message)
        {
        }
        public SaxParseException(string message, Exception ex) 
            : base(message, ex)
        {
        }
    }
}
