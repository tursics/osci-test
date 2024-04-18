using Osci.Common;

namespace Osci.Interfaces
{
    /// <exclude/>
    /// <summary>
    /// Zusammenfassung f�r ContentHandler.
    /// </summary>
    public interface IContentHandler
    {
        void StartDocument();

        void EndDocument();

        void StartPrefixMapping(string prefix, string uri);

        void StartElement(string namespaceUri, string localName, string qName, Attributes attributes);

        void EndElement(string namespaceUri, string localName, string qName);
    }
}
