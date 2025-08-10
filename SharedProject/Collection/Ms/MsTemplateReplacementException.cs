using System;
using System.Xml;

namespace FineCodeCoverage.Collection.Ms
{
    public class MsTemplateReplacementException : Exception
    {
        private readonly XmlException _innerException;
        private readonly string _replacedRunSettingsTemplate;

        public MsTemplateReplacementException(XmlException innerException, string replacedRunSettingsTemplate)
        {
            _innerException = innerException;
            _replacedRunSettingsTemplate = replacedRunSettingsTemplate;
        }

        public MsTemplateReplacementException()
            : base()
        {
        }

        public MsTemplateReplacementException(string message)
            : base(message)
        {
        }

        public MsTemplateReplacementException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public override string ToString() => $@"${_innerException} 
Replaced template :
${_replacedRunSettingsTemplate}
";
    }
}
