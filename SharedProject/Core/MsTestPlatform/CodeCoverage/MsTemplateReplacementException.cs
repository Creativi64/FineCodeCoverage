using System;
using System.Xml;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    public class MsTemplateReplacementException : Exception
    {
        private readonly XmlException _innerException;
        private readonly string _replacedRunSettingsTemplate;
        public MsTemplateReplacementException(XmlException innerException, string replacedRunSettingsTemplate)
        {
            this._innerException = innerException;
            this._replacedRunSettingsTemplate = replacedRunSettingsTemplate;
        }

        public MsTemplateReplacementException() : base()
        {
        }

        public MsTemplateReplacementException(string message) : base(message)
        {
        }

        public MsTemplateReplacementException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public override string ToString() => $@"${this._innerException} 
Replaced template :
${this._replacedRunSettingsTemplate}
";
    }
}
