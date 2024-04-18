using System.Collections.Generic;

namespace Osci.Signature
{
    /// <exclude/>
    /// <summary> Element xdsig:RetrievalMethod.
    /// 
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: PPI Financial Systems GmbH</p> 
    /// </summary>
    public class RetrievalMethod
    {
        private readonly List<string> _transformers;

        /// <summary> Standard-Konstruktor.
        /// </summary>
        public RetrievalMethod()
        {
            _transformers = new List<string>();
            Type = "http://www.w3.org/2000/09/xmldsig#X509Data";
        }

        public string[] Transformers
        {
            get
            {
                return _transformers.ToArray();
            }
        }

        /// <summary> Ruft den Wert des Attributs URI ab, oder legt diesen fest.
        /// Dieses Attribut ist REQUIRED.
        /// </summary>
        /// <value> URI.
        /// <p>@roseuid 3AA8FC2D0159; 3AA8FC2D0157</p>
        /// </value>
        public string Uri
        {
            get; set;
        }

        /// <summary> Ruft den Wert des Attributs xdsig:Type ab.
        /// </summary>
        /// <value> String.
        /// </value>
        public string Type
        {
            get;
        }

        public virtual void AddTransformer(string transformer)
        {
        }
    }
}