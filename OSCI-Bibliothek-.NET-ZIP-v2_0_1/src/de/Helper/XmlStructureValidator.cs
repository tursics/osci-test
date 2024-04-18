using Osci.Common;
using Osci.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Osci.Helper
{
    /// <summary>
    /// Simple structure validation used by XmlReader
    /// </summary>
    public class XmlStructureValidator
    {
        /// <summary>
        /// Maps node Names to their parent nodes
        /// </summary>
        private IDictionary<string, ValidationRule> _nodeHierarchy = null;

        private IDictionary<string, ValidationRule> NodeHierarchy
        {
            get
            {
                if (_nodeHierarchy == null)
                {
                    _nodeHierarchy = _createValidationRules().ToDictionary(vi => vi.ChildElement);
                }
                return _nodeHierarchy;
            }
        }

        private readonly Stack<KeyValuePair<Namespace, string>> _currentHierarchy = new Stack<KeyValuePair<Namespace, string>>();
        private readonly HashSet<string> _uniqueNodes = new HashSet<string>();

        private readonly Func<IEnumerable<ValidationRule>> _createValidationRules;

        internal XmlStructureValidator(Func<IEnumerable<ValidationRule>> validationRulesFunc)
        {
            _createValidationRules = validationRulesFunc;
        }

        /// <summary>
        /// Checks if the element is inside its respective parent element and throws an exception if not
        /// </summary>
        /// <param name="elementName"></param>
        public void ValidateNode(string elementName, string elementNamespace)
        {
            if (!IsUniquenessCorrect(elementName, elementNamespace))
            {
                throw new SaxException(string.Format("Element {0} must be unique, but duplicates were found", elementName));
            }

            if (!IsInsideRespectiveParent(elementName, elementNamespace))
            {
                string currentHierarchy = string.Join("/", _currentHierarchy.Select(kv => kv.Value).Reverse().ToArray());

                throw new SaxException(
                    string.Format("Structure Validation failed: {0} is not inside its respective parent; current hierarchy is {1}", elementName, currentHierarchy));
            }
        }

        internal void StartElement(string elementName, Namespace elementNamespace)
        {
            ValidateNode(elementName, elementNamespace);
            _currentHierarchy.Push(new KeyValuePair<Namespace, string>(elementNamespace, elementName));
        }

        internal void EndElement(string elementName, string elementNamespace)
        {
            _currentHierarchy.Pop();
        }

        private bool IsCurrentNodeChildOf(List<Namespace> parentNamespaces, string[] parentElements)
        {
            //TODO: check that new Namespace is only used for new elements?
            foreach (string parent in parentElements)
            {
                foreach (string parentNamespace in parentNamespaces)
                {
                    if (_currentHierarchy.Contains(new KeyValuePair<Namespace, string>(parentNamespace, parent)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsCurrentNodeDirectChildOf(List<Namespace> parentNamespaces, string[] parentElements)
        {
            //TODO: check that new Namespace is only used for new elements?
            KeyValuePair<Namespace, string> currentNode = GetCurrentNode();
            foreach(string parent in parentElements)
            {
                foreach(string parentNamespace in parentNamespaces)
                {
                    if (currentNode.Equals(new KeyValuePair<Namespace, string>(parentNamespace, parent)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsUniquenessCorrect(string elementName, string elementNamespace)
        {
            ValidationRule validationRule;
            if (TryGetValidationInfo(elementName, elementNamespace, out validationRule) && (validationRule.Uniqueness == ElementUniqueness.Unique))
            {
                return (_uniqueNodes.Add(elementName));
            }

            return true;
        }

        private bool IsInsideRespectiveParent(string elementNameToCheck, string elementNamespace)
        {
            ValidationRule validationRule;

            if (TryGetValidationInfo(elementNameToCheck, elementNamespace, out validationRule))
            {
                switch (validationRule.ChildValidation)
                {
                    case ChildValidation.DirectChildOfParent:
                        return IsCurrentNodeDirectChildOf(validationRule.ParentNamespaces, validationRule.ParentElements);

                    default:
                    case ChildValidation.AnywhereInsideParent:
                        return IsCurrentNodeChildOf(validationRule.ParentNamespaces, validationRule.ParentElements);
                }
            }
            else
            {
                // if respective parent is not known, everything is fine (the validation acts a a kind of blacklist; unknown elements are ignored)
                return true;
            }
        }

        private KeyValuePair<Namespace, string> GetCurrentNode()
        {
            if (_currentHierarchy.Count == 0)
            {
                return default(KeyValuePair<Namespace, string>);
            }
            return _currentHierarchy.Peek();
        }

        /// <summary>
        /// Gets the respective Parent for a given element
        /// </summary>
        /// <returns>returns default(ValidationRule) if no parent assigned</returns>
        private ValidationRule GetValidationInfo(string elemetName)
        {
            if (NodeHierarchy.ContainsKey(elemetName))
            {
                return NodeHierarchy[elemetName];
            }
            else
            {
                return default(ValidationRule);
            }
        }

        private bool TryGetValidationInfo(string elementName, string elementNamespace, out ValidationRule parentName)
        {
            if (NodeHierarchy.ContainsKey(elementName)
                && NodeHierarchy[elementName].ChildNamespace == elementNamespace)
            {
                parentName = NodeHierarchy[elementName];
                return true;
            }
            else
            {
                parentName = default(ValidationRule);
                return false;
            }
        }

        internal struct ValidationRule
        {
            public readonly string ChildElement;
            public readonly Namespace ChildNamespace;

            public readonly string[] ParentElements;
            public readonly List<Namespace> ParentNamespaces;
            public readonly ChildValidation ChildValidation;
            public readonly ElementUniqueness Uniqueness;

            internal ValidationRule(string childElement, Namespace childNamespace, ChildValidation childValidation, ElementUniqueness uniqueness, List<string> parentNamespaces, params string[] parentElements)
            {
                ChildElement = childElement;
                ChildNamespace = childNamespace;

                ParentElements = parentElements;

                ParentNamespaces = new List<Namespace>();
                foreach (string ns in parentNamespaces)
                {
                    ParentNamespaces.Add(ns);
                }
                ChildValidation = childValidation;
                Uniqueness = uniqueness;
            }

            #region predefined ValidationRules

            internal static IEnumerable<ValidationRule> CreateOsciMessageValidationRules()
            {
                yield return new ValidationRule("Body", Namespace.SoapEnvelope, ChildValidation.DirectChildOfParent, ElementUniqueness.Unique, new List<string> { Namespace.SoapEnvelope }, "Envelope");
                yield return new ValidationRule("Header", Namespace.SoapEnvelope, ChildValidation.DirectChildOfParent, ElementUniqueness.Unique, new List<string> { Namespace.SoapEnvelope }, "Envelope");
                yield return new ValidationRule("ContentPackage", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.SoapEnvelope }, "Body");
                yield return new ValidationRule("ControlBlock", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.SoapEnvelope }, "Header");
                yield return new ValidationRule("ClientSignature", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.SoapEnvelope }, "Header");
                yield return new ValidationRule("DesiredLanguages", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.SoapEnvelope }, "Header");
                yield return new ValidationRule("acceptDelivery", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.Unique, new List<string> { Namespace.SoapEnvelope }, "Header");
                yield return new ValidationRule("responseToFetchProcessCard", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.Unique, new List<string> { Namespace.SoapEnvelope }, "Body");
                yield return new ValidationRule("ProcessCardBundle", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.Osci, Namespace.Osci2017 }, "responseToPartialStoreDelivery", "responseToFetchProcessCard", "acceptDelivery", "responseToStoreDelivery", "responseToFetchDelivery", "processDelivery", "responseToForwardDelivery", "responseToFetchGOV2_CE_PROCESSCARD", "responseToFetchGOV2_CE_PROCESSCARD", "StoredMessageGOV2_CE_PROCESSCARD");
                yield return new ValidationRule("IntermediaryCertificates", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.SoapEnvelope }, "Header");
                yield return new ValidationRule("NonIntermediaryCertificates", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.SoapEnvelope }, "Header");
                yield return new ValidationRule("processDelivery", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.SoapEnvelope }, "Header");
                yield return new ValidationRule("responseToFetchDelivery", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.Unique, new List<string> { Namespace.SoapEnvelope }, "Header");
                yield return new ValidationRule("responseToPartialFetchDelivery", Namespace.Osci2017, ChildValidation.AnywhereInsideParent, ElementUniqueness.Unique, new List<string> { Namespace.SoapEnvelope }, "Header");
                yield return new ValidationRule("responseToPartialStoreDelivery", Namespace.Osci2017, ChildValidation.AnywhereInsideParent, ElementUniqueness.Unique, new List<string> { Namespace.SoapEnvelope }, "Header");
                yield return new ValidationRule("Feedback", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.Osci, Namespace.Osci2017 }, "responseToPartialFetchDelivery", "responseToPartialStoreDelivery", "InsideFeedback", "responseToFetchDelivery", "responseToFetchProcessCard", "responseToForwardDelivery", "responseToGetMessageId", "responseToMediateDelivery", "responseToExitDialog", "responseToInitDialog", "responseToStoreDelivery", "responseToFetchGOV2_CE_PROCESSCARD");
                yield return new ValidationRule("InsideFeedback", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.Osci2017 }, "responseToPartialStoreDelivery");
                yield return new ValidationRule("MessageId", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.SoapEnvelope }, "Header", "Body");
                yield return new ValidationRule("ReceptionOfDelivery", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.Osci, Namespace.Osci2017 }, "partialFetchDelivery", "fetchDelivery", "fetchProcessCard");
                yield return new ValidationRule("fetchDelivery", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.Unique, new List<string> { Namespace.SoapEnvelope }, "Header");
                yield return new ValidationRule("RecentModification", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.Osci, Namespace.Osci2017 }, "partialFetchDelivery", "fetchDelivery", "fetchProcessCard");
                yield return new ValidationRule("fetchProcessCard", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.Unique, new List<string> { Namespace.Osci }, "responseToFetchProcessCard");
                yield return new ValidationRule("Quantity", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.Osci }, "fetchProcessCard");
                yield return new ValidationRule("responseToForwardDelivery", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.Unique, new List<string> { Namespace.SoapEnvelope }, "Header");
                yield return new ValidationRule("responseToGetMessageId", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.Unique, new List<string> { Namespace.SoapEnvelope }, "Body");
                yield return new ValidationRule("responseToInitDialog", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.Unique, new List<string> { Namespace.SoapEnvelope }, "Body");
                yield return new ValidationRule("responseToMediateDelivery", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.Unique, new List<string> { Namespace.SoapEnvelope }, "Header");
                yield return new ValidationRule("RequestProcessCardBundle", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.Osci }, "responseToMediateDelivery");
                yield return new ValidationRule("ReplyProcessCardBundle", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.Osci }, "responseToMediateDelivery");
                yield return new ValidationRule("responseToStoreDelivery", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.Unique, new List<string> { Namespace.SoapEnvelope }, "Header");
                yield return new ValidationRule("MessageIdResponse", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.NotUnique, new List<string> { Namespace.SoapEnvelope }, "Header");
                yield return new ValidationRule("responseToExitDialog", Namespace.Osci, ChildValidation.AnywhereInsideParent, ElementUniqueness.Unique, new List<string> { Namespace.SoapEnvelope }, "Header", "Body");
            }

            #endregion predefined ValidationRules
        }

        internal enum ChildValidation
        {
            DirectChildOfParent,
            AnywhereInsideParent
        }

        internal enum ElementUniqueness
        {
            Unique,
            NotUnique,
        }
    }
}