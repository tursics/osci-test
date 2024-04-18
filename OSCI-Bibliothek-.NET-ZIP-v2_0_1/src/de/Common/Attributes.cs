using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Osci.Common
{
    public class Attributes
    {
        private readonly List<Attribute> _attributes;

        public int Length
        {
            get
            {
                return _attributes.Count;
            }
        }

        public bool HasValues
        {
            get
            {
                return _attributes.Count > 0;
            }
        }

        public static Attributes Empty
        {
            get
            {
                return new Attributes(null);
            }
        }


        public Attributes(IEnumerable<Attribute> attributes)
        {
            _attributes = (attributes ?? Enumerable.Empty<Attribute>()).ToList();
        }

        public string GetLocalName(int i)
        {
            if (_attributes[i].LocalName.Equals("xmlns"))
            {
                return "";
            }
            else
            {
                return _attributes[i].LocalName;
            }
        }

        public string GetQualifiedName(int i)
        {
            return _attributes[i].GetQualifiedName();
        }

        public string GetUri(int i)
        {
            return _attributes[i].NamespaceUri;
        }

        public string GetValue(int i)
        {
            return _attributes[i].Value;
        }

        public string GetValue(string key)
        {
            for (int i = 0; i < _attributes.Count; i++)
            {
                if (GetQualifiedName(i).Equals(key))
                {
                    return _attributes[i].Value;
                }
            }
            return null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Attribute attribute in _attributes)
            {
                sb.AppendFormat("{0}={1}", attribute.GetQualifiedName(), attribute.Value);
                if (!string.IsNullOrEmpty(attribute.NamespaceUri))
                {
                    sb.Append(", Namespace=" + attribute.NamespaceUri);
                }

                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}