using System;
using System.IO;
using System.Security.Cryptography;

namespace Osci.Helper
{
    public class SupportClass
    {
        /*******************************/
        /// <summary>Reads a number of characters from the current source Stream and writes the data to the target array at the specified index.</summary>
        /// <param name="sourceStream">The source Stream to read from</param>
        /// <param name="target">Contains the array of characteres read from the source Stream.</param>
        /// <param name="start">The starting index of the target array.</param>
        /// <param name="count">The maximum number of characters to read from the source Stream.</param>
        /// <returns>The number of characters read. The number will be less than or equal to count depending on the data available in the source Stream.</returns>
        [Obsolete("Use Stream.CopyTo instead.")]
        public static int ReadInput(Stream sourceStream, ref byte[] target, int start, int count)
        {
            byte[] receiver = new byte[target.Length];
            int bytesRead = sourceStream.Read(receiver, start, count);

            for (int i = start; i < start + bytesRead; i++)
            {
                target[i] = receiver[i];
            }

            if (bytesRead == 0)
            {
                return -1;
            }
            return bytesRead;
        }


        /*******************************/
        /// <summary>
        /// This class offers support for all classes that use cryptographic private keys.
        /// </summary>
        public class PrivateKeySupport 
            : KeySupport
        {
            /// <summary>
            /// Construct a new private key object
            /// </summary>
            /// 
            public string ContainerName;
            public string ProviderName;
            public int ProviderType;
            public int KeySpec;

            public PrivateKeySupport()
            {
            }
        }

        /*******************************/
        /// <summary>
        /// This class offers support for all classes that use cryptographic keys.
        /// </summary>
        public class KeySupport
        {
            private KeyedHashAlgorithm _algorithm;
            public bool IsMachineKeyset;

            /// <summary>
            /// Construct to new objects key
            /// </summary>
            public KeySupport()
            {
            }

            /// <summary>
            /// Construct to new objects key with the algorithm specified
            /// </summary>
            /// <param name="algorithm">the cryptographic algorithm</param>
            public KeySupport(KeyedHashAlgorithm algorithm)
            {
                this._algorithm = algorithm;
            }

            /// <summary>
            /// The standard algorithm name for this key
            /// </summary>
            /// <returns>the keyed hash algorithm name</returns>
            public string GetAlgorithm()
            {
                return _algorithm.ToString();
            }

            /// <summary>
            /// The key to be used in the algorithm.
            /// </summary>
            public byte[] Key
            {
                get
                {
                    return _algorithm.Key;
                }
            }
        }

        /*******************************/
        /// <summary>
        /// This class offers support for all classes that use cryptographic public keys.
        /// </summary>
        public class PublicKeySupport : KeySupport
        {
            /// <summary>
            /// Construct a new public key object
            /// </summary>
            public PublicKeySupport()
            {
            }
        }

        /*******************************/
        /// <summary>
        /// This class uses a cryptographic Random Number Generator to provide support for
        /// strong pseudo-random number generation.
        /// </summary>
        public class SecureRandomSupport
        {
            private RNGCryptoServiceProvider _generator;

            /// <summary>
            /// Initializes a new instance of the random number generator.
            /// </summary>
            public SecureRandomSupport()
            {
                _generator = new RNGCryptoServiceProvider();
            }

            /// <summary>
            /// Initializes a new instance of the random number generator with the given seed.
            /// </summary>
            /// <param name="seed">The initial seed for the generator</param>
            public SecureRandomSupport(byte[] seed)
            {
                _generator = new RNGCryptoServiceProvider(seed);
            }

            /// <summary>
            /// Returns an array of bytes with a sequence of cryptographically strong random values
            /// </summary>
            /// <param name="randomnumbersarray">The array of bytes to fill</param>
            public byte[] NextBytes(byte[] randomnumbersarray)
            {
                _generator.GetBytes(randomnumbersarray);
                return randomnumbersarray;
            }

            /// <summary>
            /// Returns the given number of seed bytes generated for the first running of a new instance 
            /// of the random number generator
            /// </summary>
            /// <param name="numberOfBytes">Number of seed bytes to generate</param>
            /// <returns>Seed bytes generated</returns>
            public static byte[] GetSeed(int numberOfBytes)
            {
                RNGCryptoServiceProvider generatedSeed = new RNGCryptoServiceProvider();
                byte[] seeds = new byte[numberOfBytes];
                generatedSeed.GetBytes(seeds);
                return seeds;
            }

            /// <summary>
            /// Creates a new instance of the random number generator with the seed provided by the user
            /// </summary>
            /// <param name="newSeed">Seed to create a new random number generator</param>
            public void SetSeed(byte[] newSeed)
            {
                _generator = new RNGCryptoServiceProvider(newSeed);
            }

            /// <summary>
            /// Creates a new instance of the random number generator with the seed provided by the user
            /// </summary>
            /// <param name="newSeed">Seed to create a new random number generator</param>
            public void SetSeed(long newSeed)
            {
                byte[] bytes = new byte[8];
                for (int index = 7; index > 0; index--)
                {
                    bytes[index] = (byte)(newSeed - (long)((newSeed >> 8) << 8));
                    newSeed = (long)(newSeed >> 8);
                }
                SetSeed(bytes);
            }
        }


        /*******************************/
        /// <summary>
        /// Removes the element with the specified key from a Hashtable instance.
        /// </summary>
        /// <param name="hashtable">The Hashtable instance</param>
        /// <param name="key">The key of the element to remove</param>
        /// <returns>The element removed</returns>  
        public static object HashtableRemove(System.Collections.Hashtable hashtable, object key)
        {
            object element = hashtable[key];
            hashtable.Remove(key);
            return element;
        }

   

        /*******************************/
        public class CollectionSupport
        {
            /// <summary>
            /// Obtains an array containing all the elements of the collection.
            /// </summary>
            /// <param name="objects">The array into which the elements of the collection will be stored.</param>
            /// <returns>The array containing all the elements of the collection.</returns>
            public static object[] ToArray(System.Collections.ICollection c, object[] objects)
            {
                int index = 0;

                Type type = objects.GetType().GetElementType();
                object[] objs = (object[])Array.CreateInstance(type, c.Count);

                System.Collections.IEnumerator e = c.GetEnumerator();

                while (e.MoveNext())
                    objs[index++] = e.Current;

                //If objects is smaller than c then do not return the new array in the parameter
                if (objects.Length >= c.Count)
                    objs.CopyTo(objects, 0);

                return objs;
            }
        
        }
        public class DateTimeFormatManager
        {
            static public DateTimeFormatHashTable Manager = new DateTimeFormatHashTable();

            public class DateTimeFormatHashTable 
                : System.Collections.Hashtable
            {
                public void SetDateFormatPattern(System.Globalization.DateTimeFormatInfo format, string newPattern)
                {
                    if (this[format] != null)
                        ((DateTimeFormatProperties)this[format]).DateFormatPattern = newPattern;
                    else
                    {
                        DateTimeFormatProperties tempProps = new DateTimeFormatProperties();
                        tempProps.DateFormatPattern = newPattern;
                        Add(format, tempProps);
                    }
                }

                public string GetDateFormatPattern(System.Globalization.DateTimeFormatInfo format)
                {
                    if (this[format] == null)
                        return "d-MMM-yy";
                    else
                        return ((DateTimeFormatProperties)this[format]).DateFormatPattern;
                }

                public void SetTimeFormatPattern(System.Globalization.DateTimeFormatInfo format, string newPattern)
                {
                    if (this[format] != null)
                        ((DateTimeFormatProperties)this[format]).TimeFormatPattern = newPattern;
                    else
                    {
                        DateTimeFormatProperties tempProps = new DateTimeFormatProperties();
                        tempProps.TimeFormatPattern = newPattern;
                        Add(format, tempProps);
                    }
                }

                public string GetTimeFormatPattern(System.Globalization.DateTimeFormatInfo format)
                {
                    if (this[format] == null)
                        return "h:mm:ss tt";
                    else
                        return ((DateTimeFormatProperties)this[format]).TimeFormatPattern;
                }

                class DateTimeFormatProperties
                {
                    public string DateFormatPattern = "d-MMM-yy";
                    public string TimeFormatPattern = "h:mm:ss tt";
                }
            }
        }

        /*******************************/
        public static string FormatDateTime(System.Globalization.DateTimeFormatInfo format, DateTime date)
        {
            string timePattern = DateTimeFormatManager.Manager.GetTimeFormatPattern(format);
            string datePattern = DateTimeFormatManager.Manager.GetDateFormatPattern(format);
            return date.ToString(datePattern + " " + timePattern, format);
        }

        /*******************************/
        public class Tokenizer
        {
            private System.Collections.ArrayList _elements;
            private string _source;
            //The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character
            private string _delimiters = " \t\n\r";


            public Tokenizer(string source, string delimiters)
            {
                _elements = new System.Collections.ArrayList();
                this._delimiters = delimiters;
                _elements.AddRange(source.Split(this._delimiters.ToCharArray()));
                RemoveEmptyStrings();
                this._source = source;
            }


            public bool HasMoreTokens()
            {
                return (_elements.Count > 0);
            }

            public string NextToken()
            {
                string result;
                if (_source == "")
                    throw new Exception();
                else
                {
                    _elements = new System.Collections.ArrayList();
                    _elements.AddRange(_source.Split(_delimiters.ToCharArray()));
                    RemoveEmptyStrings();
                    result = (string)_elements[0];
                    _elements.RemoveAt(0);
                    _source = _source.Remove(_source.IndexOf(result), result.Length);
                    _source = _source.TrimStart(_delimiters.ToCharArray());
                    return result;
                }
            }

            private void RemoveEmptyStrings()
            {
                //VJ++ does not treat empty strings as tokens
                for (int index = 0; index < _elements.Count; index++)
                    if ((string)_elements[index] == "")
                    {
                        _elements.RemoveAt(index);
                        index--;
                    }
            }
        }
    }
}
