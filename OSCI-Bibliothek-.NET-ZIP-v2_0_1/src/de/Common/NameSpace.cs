namespace Osci.Common
{
    internal struct Namespace
    {
        #region Predefined Namespaces
        /// <summary>
        /// http://schemas.xmlsoap.org/soap/envelope/
        /// </summary>
        public const string SoapEnvelope = "http://schemas.xmlsoap.org/soap/envelope/";

        /// <summary>
        /// http://www.osci.de/2002/04/osci
        /// </summary>
        public const string Osci = "http://www.osci.de/2002/04/osci";

        /// <summary>
        /// http://xoev.de/transport/osci12/7
        /// </summary>
        public const string Osci2017 = "http://xoev.de/transport/osci12/7";

        /// <summary>
        /// http://xoev.de/transport/osci12/8
        /// </summary>
        public const string Osci128 = "http://xoev.de/transport/osci12/8";

        /// <summary>
        /// http://www.w3.org/2000/09/xmldsig#
        /// </summary>
        public const string XmlDSig = "http://www.w3.org/2000/09/xmldsig#";

        /// <summary>
        /// http://www.w3.org/2001/04/xmlenc#
        /// </summary>
        public const string XmlEnc = "http://www.w3.org/2001/04/xmlenc#";

        /// <summary>
        /// http://www.w3.org/2001/XMLSchema-instance
        /// </summary>
        public const string XsiSchema = "http://www.w3.org/2001/XMLSchema-instance";
        #endregion

        private readonly string _namespaceUri;

        public Namespace(string namespaceUri)
        {
            _namespaceUri = namespaceUri.ToLowerInvariant();
        }

        public static implicit operator string(Namespace ns)
        {
            return ns._namespaceUri;
        }

        public static implicit operator Namespace(string namespaceStr)
        {
            return new Namespace(namespaceStr);
        }

        public static bool operator==(Namespace left, Namespace right)
        {
            return (left._namespaceUri == right._namespaceUri);
        }

        public static bool operator !=(Namespace left, Namespace right)
        {
            return (left._namespaceUri != right._namespaceUri);
        }

        public override bool Equals(object obj)
        {
            if (obj is string)
            {
                string other = ((string) obj).ToLowerInvariant();
                return (_namespaceUri == other);
            }
            else if (obj is Namespace)
            {
                Namespace other = (Namespace) obj;
                return (this._namespaceUri == other._namespaceUri);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _namespaceUri.GetHashCode();
        }

        public override string ToString()
        {
            return _namespaceUri;
        }
    }
}
