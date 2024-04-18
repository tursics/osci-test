using System;
using Osci.Extensions;

namespace Osci.Common
{
    public enum AsymmetricCipherAlgorithm
    {
        [AlgorithmInfo(XmlName = Constants.AsymmetricCipherAlgorithmRsa15)]
        Rsa15,
        [AlgorithmInfo(XmlName = Constants.AsymmetricCipherAlgorithmRsaOaep)]
        RsaOaep
    }

    public static class AsymmetricCipherAlgorithmExtensions
    {
        internal static string GetXmlName(this AsymmetricCipherAlgorithm e)
        {
            return ((Enum)e).GetXmlName();
        }
    }
}