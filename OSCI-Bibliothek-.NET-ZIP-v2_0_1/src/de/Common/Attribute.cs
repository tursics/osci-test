namespace Osci.Common
{
    public class Attribute
    {
        public string Prefix
        {
            get;
        }

        public string LocalName
        {
            get;
        }

        public string NamespaceUri
        {

            get;
        }

        public string Value
        {
            get;
        }

        public Attribute(string prefix, string localName, string namespaceUri, string value)
        {
            Prefix = prefix;
            LocalName = localName;
            NamespaceUri = namespaceUri;
            Value = value;
        }

        public string GetQualifiedName()
        {
            if (string.IsNullOrEmpty(Prefix))
            {
                return LocalName;
            }
            return Prefix + ":" + LocalName;
        }
    }
}