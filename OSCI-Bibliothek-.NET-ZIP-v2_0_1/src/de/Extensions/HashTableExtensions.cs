using System.Collections;

namespace Osci.Extensions
{
    internal static class HashTableExtensions
    {
        public static object Put(this Hashtable hashTable, object key, object value)
        {
            object currentValue = hashTable[key];
            hashTable[key] = value;
            return currentValue;
        }
    }
}
