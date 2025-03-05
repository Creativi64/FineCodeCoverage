using System;
using System.Xml;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    public class MsTemplateReplacementException : Exception
    {
        private readonly XmlException innerException;
        private readonly string replacedRunSettingsTemplate;
        public MsTemplateReplacementException(XmlException innerException, string replacedRunSettingsTemplate)
        {
            this.innerException = innerException;
            this.replacedRunSettingsTemplate = replacedRunSettingsTemplate;
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

        public override string ToString()
        {
            return $@"${innerException} 
Replaced template :
${replacedRunSettingsTemplate}
";
        }

    }
}
