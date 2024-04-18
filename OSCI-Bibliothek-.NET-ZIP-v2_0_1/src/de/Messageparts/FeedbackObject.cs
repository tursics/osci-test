using Osci.Helper;

namespace Osci.MessageParts
{
	/// <summary><H4>FeedbackObject</H4>
	/// <p>Diese Klasse repräsentiert die Feedback-Einträge einer OSCIResponseTo-Nachricht</p>
	/// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
	/// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
	/// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
	/// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
	/// 
	/// <p>Author: P. Ricklefs, N. Büngener</p> 
	/// <p>Version: 2.0.1</p>
	/// </summary>
	public class FeedbackObject
	{
		private static Log _log = LogFactory.GetLog(typeof(FeedbackObject));
		private string _lang, _code, _text;

		internal FeedbackObject(string[] feedback) {
			_lang = feedback[0];
			_code = feedback[1];
			_text = feedback[2];
		}

		/// <summary> Liefert die Sprachkürzel der Feedbackeinträge (de, en...).
		/// </summary>
		/// <value> Sprachkürzel
		/// </value>
		public string Language {get{return _lang;}}

		/// <summary> Liefert die Rückmeldecodes der Feedbackeinträge.
		/// </summary>
		/// <value> Rückmeldecodes
		/// </value>
		public string Code {get{return _code;}}

		/// <summary> Liefert die Texte der Feedbackeinträge.
		/// </summary>
		/// <value> Texte
		/// </value>
		public string Text {get{return _text;}}
	}
}
