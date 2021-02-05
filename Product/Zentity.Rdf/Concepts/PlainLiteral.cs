// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Concepts
{
    /// <summary>
    /// This class represents an RDF plain literal 
    /// (http://www.w3.org/TR/2004/REC-rdf-concepts-20040210/#dfn-plain-literal). 
    /// Plain literals have a lexical form and optionally a language tag.
    /// </summary>
    public sealed class PlainLiteral : Literal
    {
        #region Member Variables

        #region Private
        string languageTag;
        #endregion

        #endregion

        #region Properties

        #region Public
        /// <summary>
        /// Gets the language tag for a plain literal as defined by 
        /// RFC-3066, normalized to lowercase.
        /// </summary>
        public string LanguageTag
        {
            get { return languageTag; }
        }
        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the PlainLiteral class. This constructor initializes itself using input parameter.
        /// </summary>
        /// <param name="lexicalForm">Lexical form.</param>
        public PlainLiteral(string lexicalForm) : base(lexicalForm) { }

        /// <summary>
        /// Initializes a new instance of the PlainLiteral class. This constructor initializes itself using input parameter.
        /// </summary>
        /// <param name="lexicalForm">Lexical form.</param>
        /// <param name="languageTag">Language tag.</param>
        public PlainLiteral(string lexicalForm, string languageTag)
            : base(lexicalForm)
        {
            this.languageTag = languageTag;
        }

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Returns concatinated string of lexical form and language tag in N-Triple format.
        /// </summary>
        /// <returns>Concatinated string of lexical form and LanguageTag in N-Triple format.</returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(languageTag))
                return "\"" + this.LexicalForm + "\"";
            else
                return "\"" + this.LexicalForm + "\"@" + languageTag;
        }

        #region ICloneable Members

        /// <summary>
        /// Creates a new PlainLiteral object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new PlainLiteral object that is a copy of the current instance.</returns>
        public override object Clone()
        {
            PlainLiteral clonedLiteral = new PlainLiteral(this.LexicalForm, this.LanguageTag);

            return clonedLiteral;
        }

        #endregion

        #endregion

        #endregion
 
    }
}
