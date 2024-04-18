using Osci.MessageParts;

namespace Osci.SoapHeader
{
    /// <exclude/>
    /// <summary>
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public abstract class HeaderEntry
        : MessagePart
    {
        protected HeaderEntry()
        {
            RefId = GetType().Name.TrimEnd('H').ToLowerInvariant();
            Transformers.Add(Canonizer);
        }
    }
}