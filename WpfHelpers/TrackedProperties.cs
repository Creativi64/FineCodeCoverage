using System.Collections.Generic;
using System.Windows;

namespace WpfHelpers
{
    public class TrackedProperties
    {
        public TrackedProperties(DependencyProperty trackedProperty, List<DependencyProperty> trackingProperties)
        {
            TrackedProperty = trackedProperty;
            TrackingProperties = trackingProperties;
        }

        public DependencyProperty TrackedProperty { get; }
        public List<DependencyProperty> TrackingProperties { get; }
    }
}
