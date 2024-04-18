using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Osci.Common;
using Osci.Exceptions;

namespace Osci.Helper
{
    internal class UniqueElementTracker
    {
        private readonly HashSet<string> _elementNames = new HashSet<string>();
        private readonly HashSet<string> _ids = new HashSet<string>();

        public void AddElement(string localName, string @namespace, Attributes attributes)
        {
            Add(attributes.GetValue("Id"), CreateQualifiedName(localName, @namespace));
        }

        public void AddElement(string localName, string @namespace, Attributes attributes, string additionalDescriptor)
        {
            Add(attributes.GetValue("Id"), CreateQualifiedName(localName, @namespace, additionalDescriptor));
        }

        public bool IsSubsetOf(IEnumerable<string> ids)
        {
            List<string> superset = ids.ToList();
            return _ids.All(id => superset.Contains(id));
        }

        public bool IsSubsetOf(IEnumerable ids)
        {
            return IsSubsetOf(ids.Cast<string>());
        }

        private static string CreateQualifiedName(string localName, string @namespace, string additionalDescriptor = null)
        {
            return string.IsNullOrEmpty(additionalDescriptor)
                ? @namespace + ":" + localName
                : @namespace + ":" + localName + "_" + additionalDescriptor;
        }

        private void Add(string idValue, string qualifiedName)
        {
            if (!_elementNames.Add(qualifiedName))
            {
                throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry"));
            }

            if (idValue == null)
            {
                throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry"));
            }

            idValue = "#" + idValue;

            if (!_ids.Add(idValue))
            {
                throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry"));
            }
        }
    }
}
