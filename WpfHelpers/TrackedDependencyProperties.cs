using System;
using System.Linq;
using System.Windows;

namespace WpfHelpers
{
    public static class TrackedDependencyProperties
    {
        public static void Track<TOwner, TPropertyType>((string name, Action<DependencyProperty> setter) trackedName, TPropertyType trackedDefault, params (string name, Action<DependencyProperty> setter)[] trackingNames)
        {
            DependencyProperty trackedDependencyProperty = null;
            var trackingDps = trackingNames.Select(trackingName =>
            {
                PropertyMetadata propertyMetadata = null;
                CoerceValueCallback coerceValueCallback = (d, baseValue) =>
                {
                    if (propertyMetadata.DefaultValue == baseValue)
                    {
                        return d.GetValue(trackedDependencyProperty);
                    }
                    return baseValue;
                };
                propertyMetadata = new PropertyMetadata(trackedDefault, null, coerceValueCallback);


                var trackingDependencyProperty = DependencyProperty.Register(trackingName.name, typeof(TPropertyType), typeof(TOwner), propertyMetadata);
                trackedName.setter(trackingDependencyProperty);
                return trackingDependencyProperty;
            }).ToList();
            trackedDependencyProperty = DependencyProperty.Register(trackedName.name, typeof(TPropertyType), typeof(TOwner), new PropertyMetadata(trackedDefault, (d, e) =>
            {
                foreach (var trackingDp in trackingDps)
                {
                    d.CoerceValue(trackingDp);
                }
            }));
            trackedName.setter(trackedDependencyProperty);
        }
        public static TrackedProperties Track<TOwner, TPropertyType>(string trackedName, TPropertyType trackedDefault, params string[] trackingNames)
        {
            DependencyProperty trackedDependencyProperty = null;
            var trackingDps = trackingNames.Select(trackingName =>
            {
                PropertyMetadata propertyMetadata = null;
                CoerceValueCallback coerceValueCallback = (d, baseValue) =>
                {
                    if (propertyMetadata.DefaultValue == baseValue)
                    {
                        return d.GetValue(trackedDependencyProperty);
                    }
                    return baseValue;
                };
                propertyMetadata = new PropertyMetadata(trackedDefault, null, coerceValueCallback);


                return DependencyProperty.Register(trackingName, typeof(TPropertyType), typeof(TOwner), propertyMetadata);
            }).ToList();
            trackedDependencyProperty = DependencyProperty.Register(trackedName, typeof(TPropertyType), typeof(TOwner), new PropertyMetadata(trackedDefault, (d, e) =>
            {
                foreach (var trackingDp in trackingDps)
                {
                    d.CoerceValue(trackingDp);
                }
            }));
            return new TrackedProperties(trackedDependencyProperty, trackingDps);

        }
    }
}
