using Osci.Exceptions;

namespace Osci.Interfaces
{
    /// <exclude/>
    /// <summary>
    /// Zusammenfassung für ErrorHandler.
    /// </summary>
    /// 
    public interface IErrorHandler
    {

        void Warning(SaxParseException exception);


        /**
         * Receive notification of a recoverable error.
         *
         * <p>This corresponds to the definition of "error" in section 1.2
         * of the W3C XML 1.0 Recommendation.  For example, a validating
         * parser would use this callback to report the violation of a
         * validity constraint.  The default behaviour is to take no
         * action.</p>
         *
         * <p>The SAX parser must continue to provide normal parsing events
         * after invoking this method: it should still be possible for the
         * application to process the document through to the end.  If the
         * application cannot do so, then the parser should report a fatal
         * error even if the XML 1.0 recommendation does not require it to
         * do so.</p>
         *
         * <p>Filters may use this method to report other, non-XML errors
         * as well.</p>
         *
         * @param exception The error information encapsulated in a
         *                  SAX parse exception.
         * @exception org.xml.sax.SAXException Any SAX exception, possibly
         *            wrapping another exception.
         * @see org.xml.sax.SAXParseException 
         */
        void Error(SaxParseException exception);


        /**
         * Receive notification of a non-recoverable error.
         *
         * <p>This corresponds to the definition of "fatal error" in
         * section 1.2 of the W3C XML 1.0 Recommendation.  For example, a
         * parser would use this callback to report the violation of a
         * well-formedness constraint.</p>
         *
         * <p>The application must assume that the document is unusable
         * after the parser has invoked this method, and should continue
         * (if at all) only for the sake of collecting addition error
         * messages: in fact, SAX parsers are free to stop reporting any
         * other events once this method has been invoked.</p>
         *
         * @param exception The error information encapsulated in a
         *                  SAX parse exception.  
         * @exception org.xml.sax.SAXException Any SAX exception, possibly
         *            wrapping another exception.
         * @see org.xml.sax.SAXParseException
         */
        void FatalError(SaxParseException exception);

    }
}
