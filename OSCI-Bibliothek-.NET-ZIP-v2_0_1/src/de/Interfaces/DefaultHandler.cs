using Osci.Common;
using Osci.Exceptions;

namespace Osci.Interfaces
{
    /// <exclude/>
    /// <summary>
    /// Zusammenfassung f�r DefaultHandler.
    /// </summary>
    public abstract class DefaultHandler
        : IContentHandler
        , IErrorHandler
    {
        private string _currentElement;

        internal bool IsDuplicateIdCheckEnabled
        {
            get;
            set;
        }

        protected DefaultHandler()
        {
            IsDuplicateIdCheckEnabled = true;
        }


        public virtual void StartPrefixMapping(string prefix, string uri)
        {
        }

        public virtual void StartDocument()
        {
        }

        public virtual void EndElement(string uri, string localName, string qName)
        {
        }

        public virtual void Characters(char[] ch, int start, int length)
        {
        }

        public virtual void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
        }

        public virtual void EndDocument()
        {
        }

        public virtual void Warning(SaxParseException exception)
        {
        }

        public virtual void Error(SaxParseException exception)
        {
        }

        public virtual void FatalError(SaxParseException exception)
        {
        }
    }
}
