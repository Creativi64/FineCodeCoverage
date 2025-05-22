using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OptionsExtractor
{
    internal class OptionPageInfoProvider : IOptionPageInfoProvider
    {
        public IEnumerable<OptionPageInfo> Provide(Type packageType)
        {
            PropertyInfo pageTypePropertyInfo = null;
            PropertyInfo pageNamePropertyInfo = null;

            var provideOptionPageAttributes = packageType.GetCustomAttributes().Where(a => a.GetType().Name == "ProvideOptionPageAttribute");
            return provideOptionPageAttributes.Select(provideOptionPageAttribute =>
            {
                var pageTypeAndName = GetPageTypeAndName(provideOptionPageAttribute);
                return new OptionPageInfo(
                    pageTypeAndName.pageType.BaseType.GetGenericArguments()[0],
                    pageTypeAndName.pageName);
            });

            (Type pageType,string pageName) GetPageTypeAndName(Attribute attribute)
            {
                if(pageTypePropertyInfo == null)
                {
                    pageTypePropertyInfo = attribute.GetType().GetProperty("PageType");
                }
                if(pageNamePropertyInfo == null)
                {
                    pageNamePropertyInfo = attribute.GetType().GetProperty("PageName");
                }
                return ((Type)pageTypePropertyInfo.GetValue(attribute), (string)pageNamePropertyInfo.GetValue(attribute));
            }
        }

    }
}
