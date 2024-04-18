using System;
using System.Linq;
using Osci.Common;
using Attribute = System.Attribute;

namespace Osci.Extensions
{
    internal static class EnumExtensions
    {
        internal static string GetXmlName(this Enum e)
        {
            var attribute = e.GetAttribute<AlgorithmInfoAttribute>();
            return attribute == null ? null : attribute.XmlName;
        }

        public static bool IsEqualTo(this Enum e, string xmlName)
        {
            var attribute = e.GetAttribute<AlgorithmInfoAttribute>();
            return attribute != null && string.Equals(attribute.XmlName, xmlName);
        }

        internal static TAttribute GetAttribute<TAttribute>(this Enum value)
            where TAttribute : Attribute
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            return type.GetField(name) // I prefer to get attributes this way
                .GetCustomAttributes(false)
                .OfType<TAttribute>()
                .SingleOrDefault();
        }
    }
}