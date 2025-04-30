using System.Windows.Controls;
using System.Windows.Media;
using System;
using System.Windows;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Navigation;

namespace FineCodeCoverage.Output
{
    public enum CoveragePercentageBarStyle { Percent, Covered, NotCovered }

    public class DependentPropertiesDescriptor
    {
        private DependencyPropertyDescriptor dependencyPropertyDescriptor;
        private Func<string, IEnumerable<string>> _getDependentProperties;
        public DependentPropertiesDescriptor(
            DependencyPropertyDescriptor dependencyPropertyDescriptor, 
            Func<string, IEnumerable<string>> getDependentProperties
            )
        {
            this.dependencyPropertyDescriptor = dependencyPropertyDescriptor;
            _getDependentProperties = getDependentProperties;
        }

        public void AddValueChanged(object instance, EventHandler handler)
        {
            this.dependencyPropertyDescriptor.AddValueChanged(instance, handler);
        }

        public void RemoveValueChanged(object instance, EventHandler handler)
        {
            this.dependencyPropertyDescriptor.RemoveValueChanged(instance, handler);
        }

        public IEnumerable<string> GetDependentProperties()
        {
            return _getDependentProperties(dependencyPropertyDescriptor.Name);
        }

        
    }

    public class DependentPropertiesChangedNotifier<T> where T : IPropertyDependencyChanged
    {
        private readonly List<DependentPropertiesDescriptor> dependentPropertiesDescriptors;
        private Dictionary<T, EventHandler> handlers = new Dictionary<T, EventHandler>();

        public DependentPropertiesChangedNotifier(List<DependentPropertiesDescriptor> dependentPropertiesDescriptors)
        {
            this.dependentPropertiesDescriptors = dependentPropertiesDescriptors;
        }

        private EventHandler CreateHandler(T instance, DependentPropertiesDescriptor dependentPropertiesDescriptor)
        {
            EventHandler handler = (_, __) =>
            {
                var dependentPropertyNames = dependentPropertiesDescriptor.GetDependentProperties();
                foreach (var propertyName in dependentPropertyNames)
                {
                    instance.NotifyDependentPropertyChanged(propertyName);
                }
            };
            handlers.Add(instance, handler);
            return handler;
        }

        public void NotifyOrChanges(T instance)
        {
            this.dependentPropertiesDescriptors.ForEach(dependentPropertiesDescriptor =>
            {
                dependentPropertiesDescriptor.AddValueChanged(instance, CreateHandler(instance, dependentPropertiesDescriptor));
            });
        }


        public void RemoveNotificationOfChanges(T instance)
        {
            this.dependentPropertiesDescriptors.ForEach(dependentPropertiesDescriptor =>
            {
                dependentPropertiesDescriptor.RemoveValueChanged(instance, handlers[instance]);
            });
        }
        // need to remove as well
    }


    public static class DependencyPropertyHelper
    {
        public static DependentPropertiesChangedNotifier<T> Build<T>() where T : IPropertyDependencyChanged
        {
            var type = typeof(T);
            var dependencies = BuildPropertyDependencies(type);
            var dependencyPropertyFields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).Where(f => f.FieldType == typeof(DependencyProperty));
            var dependencyProperties = dependencyPropertyFields.Select(f => f.GetValue(null) as DependencyProperty).ToList();
            var propertyNamesDependOn = dependencies.Keys;
            var allDescriptorDependents = dependencies.Select(kvp =>
            {
                var dependedUponDp = dependencyProperties.First(dp => dp.Name == kvp.Key);
                var descriptor = DependencyPropertyDescriptor.FromProperty(dependedUponDp, type);

                return new DependentPropertiesDescriptor(descriptor, (changedPropertyName) =>
                {
                    return GetDependentProperties(changedPropertyName, dependencies);
                });
            }).ToList();
            return new DependentPropertiesChangedNotifier<T>(allDescriptorDependents);
            // no reason why could not depen

        }
        public static IDictionary<string, OneOrMany<string>> BuildPropertyDependencies(Type type)
        {
            IDictionary<string, OneOrMany<string>> map1 = (IDictionary<string, OneOrMany<string>>)null;
            IDictionary<string, OneOrMany<string>> map2 = (IDictionary<string, OneOrMany<string>>)null;
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            HashSet<string> stringSet = new HashSet<string>(((IEnumerable<PropertyInfo>)properties).Select((Func<PropertyInfo, string>)(prop => prop.Name)));
            foreach (PropertyInfo propertyInfo in properties)
            {
                foreach (string str in Enumerable.Cast<DependsOnPropertyAttribute>(propertyInfo.GetCustomAttributes(typeof(DependsOnPropertyAttribute), true)).Select((attr => attr.PropertyName)))
                {
                    if (!stringSet.Contains(str))
                        throw new DependsOnPropertyNotFoundException(propertyInfo.DeclaringType, propertyInfo.Name, str);
                    AddToMapValues(ref map2, str, propertyInfo.Name);
                    AddToMapValues(ref map1, propertyInfo.Name, str);
                }
            }
            ValidatePropertyDependencies(map1);
            return map2;

            void AddToMapValues(
              ref IDictionary<string, OneOrMany<string>> map,
              string key,
              string valueToAdd)
            {
                if (map == null)
                    map = new Dictionary<string, OneOrMany<string>>();
                OneOrMany<string> oneOrMany;
                map.TryGetValue(key, out oneOrMany);
                oneOrMany.Add(valueToAdd);
                map[key] = oneOrMany;
            }
        }
        private static void ValidatePropertyDependencies(
  IDictionary<string, OneOrMany<string>> propertyDependencies)
        {
            if (propertyDependencies == null)
                return;
            List<string> allDependentProperties = new List<string>();
            foreach (string key in (IEnumerable<string>)propertyDependencies.Keys)
            {
                allDependentProperties.Clear();
                AddDependentProperties(key, key, propertyDependencies, ref allDependentProperties);
            }
        }

        private static bool AddDependentProperties(
          string rootProperty,
          string property,
          IDictionary<string, OneOrMany<string>> propertyDependencies,
          ref List<string> allDependentProperties)
        {
            OneOrMany<string> oneOrMany;
            if (propertyDependencies == null || !propertyDependencies.TryGetValue(property, out oneOrMany))
                return false;
            foreach (string str in oneOrMany)
            {
                if (allDependentProperties == null)
                    allDependentProperties = new List<string>();
                if (!allDependentProperties.Contains(str))
                {
                    allDependentProperties.Add(str);
                    if (str == rootProperty)
                        throw new CircularPropertyDependencyException(str, allDependentProperties.ToArray());
                    AddDependentProperties(rootProperty, str, propertyDependencies, ref allDependentProperties);
                }
                else
                    break;
            }
            return true;
        }

        public static IEnumerable<string> GetDependentProperties(string property, IDictionary<string, OneOrMany<string>> _propertyDependencies)
        {
            List<string> allDependentProperties = (List<string>)null;
            return !AddDependentProperties(property, property, _propertyDependencies, ref allDependentProperties) ? Enumerable.Empty<string>() : (IEnumerable<string>)allDependentProperties;
        }
    }

    public interface IPropertyDependencyChanged
    {
        void NotifyDependentPropertyChanged(string propertyName);
    }

    public partial class CoveragePercentageBar : UserControl, INotifyPropertyChanged, IPropertyDependencyChanged
    {
        private static DependentPropertiesChangedNotifier<CoveragePercentageBar> dpInfo;
        public CoveragePercentageBar()
        {
            InitializeComponent();
            if(dpInfo == null)
            {
                dpInfo = DependencyPropertyHelper.Build<CoveragePercentageBar>();
            }
            // could wait for load event
            dpInfo.NotifyOrChanges(this);
        }

        public void NotifyDependentPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged == null)
                return;

           propertyChanged((object)this, new PropertyChangedEventArgs(propertyName));
        }

        [DependsOnProperty(nameof(CoveragePercentageBarStyle))]
        public bool ADp => CoveragePercentageBarStyle == CoveragePercentageBarStyle.Covered;

        public CoveragePercentageBarStyle CoveragePercentageBarStyle
        {
            get { return (CoveragePercentageBarStyle)GetValue(CoveragePercentageBarStyleProperty); }
            set { SetValue(CoveragePercentageBarStyleProperty, value); }
        }

        public static readonly DependencyProperty CoveragePercentageBarStyleProperty =
            DependencyProperty.Register(nameof(CoveragePercentageBarStyle), typeof(CoveragePercentageBarStyle), typeof(CoveragePercentageBar), new PropertyMetadata(CoveragePercentageBarStyle.Percent));

        public bool UseSolidBrush
        {
            get { return (bool)GetValue(UseSolidBrushProperty); }
            set { SetValue(UseSolidBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UseSolidBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UseSolidBrushProperty =
            DependencyProperty.Register(nameof(UseSolidBrush), typeof(bool), typeof(CoveragePercentageBar), new PropertyMetadata(true));


        public int? Partial
        {
            get { return (int?)GetValue(PartialProperty); }
            set { SetValue(PartialProperty, value); }
        }

        public static readonly DependencyProperty PartialProperty =
            DependencyProperty.Register(nameof(Partial), typeof(int?), typeof(CoveragePercentageBar), new PropertyMetadata(null, CalculatePercentage));

        public double Coverable
        {
            get { return (double)GetValue(CoverableProperty); }
            set { SetValue(CoverableProperty, value); }
        }

        public static readonly DependencyProperty CoverableProperty =
            DependencyProperty.Register(nameof(Coverable), typeof(double), typeof(CoveragePercentageBar), new PropertyMetadata((double)0,CalculatePercentage));

        public double Covered
        {
            get { return (double)GetValue(CoveredProperty); }
            set { SetValue(CoveredProperty, value); }
        }

        public static readonly DependencyProperty CoveredProperty =
            DependencyProperty.Register(nameof(Covered), typeof(double), typeof(CoveragePercentageBar), new PropertyMetadata((double)0, CalculatePercentage));

        public double Percentage
        {
            get { return (double)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }

        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register(nameof(Percentage), typeof(double), typeof(CoveragePercentageBar), new PropertyMetadata((double)0));

        private static void CalculatePercentage(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var coveragePercentageBar = d as CoveragePercentageBar;
            coveragePercentageBar.CalculatePercentage();
            // if no coverable then should style specifically
        }
        private void CalculatePercentage()
        {
            if(Coverable != 0)
            {
                Percentage = Covered / Coverable;
            }

            SetCoverageToolTip();
        }

        private void SetCoverageToolTip()
        {
            if (Coverable != 0)
            {
                var percentageRounded = Math.Round(Percentage * 100, 2);
                if (Partial.HasValue)
                {
                    var partialValue = Partial.Value;
                    var uncovered = Coverable - Covered - Partial;
                    CoverageTooltip =
    $@"{percentageRounded} %
Covered     - {Covered}
Uncovered - {uncovered}
Partial       - {partialValue}
";
                }
                else
                {
                    CoverageTooltip = $"{percentageRounded} % - {Covered} / {Coverable}";
                }
            }
            else
            {
                CoverageTooltip = "No coverable";
            }
        }

        public string CoverageTooltip
        {
            get { return (string)GetValue(CoverageTooltipProperty); }
            set { SetValue(CoverageTooltipProperty, value); }
        }

        public static readonly DependencyProperty CoverageTooltipProperty =
            DependencyProperty.Register(nameof(CoverageTooltip), typeof(string), typeof(CoveragePercentageBar), new PropertyMetadata(""));

        public bool CoveredPercentageLeft
        {
            get { return (bool)GetValue(CoveredPercentageLeftProperty); }
            set { SetValue(CoveredPercentageLeftProperty, value); }
        }

        public static readonly DependencyProperty CoveredPercentageLeftProperty =
            DependencyProperty.Register(nameof(CoveredPercentageLeft), typeof(bool), typeof(CoveragePercentageBar), new PropertyMetadata(false));


        public static readonly DependencyProperty ThemedBackgroundColorProperty =
        DependencyProperty.Register(
            nameof(ThemedBackgroundColor),
            typeof(Color),
            typeof(CoveragePercentageBar),
            new PropertyMetadata(Colors.Transparent, OnBackgroundColorChanged));

        public Color ThemedBackgroundColor
        {
            get => (Color)GetValue(ThemedBackgroundColorProperty);
            set => SetValue(ThemedBackgroundColorProperty, value);
        }

        private static void OnBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // if supply one then theme automatically
            var control = (CoveragePercentageBar)d;
            control.BackgroundColorChanged();
        }

        private void BackgroundColorChanged()
        {
            if(ThemedBackgroundColor == Colors.Transparent)
            {
                CoverageBorderThicknessActual = CoverageBarBorderThickness;
            }
            else
            {
                CoverageBorderThicknessActual = new Thickness(0);
            }
            UpdateBrush(false);
            UpdateBrush(true);
        }

        public Thickness CoverageBarBorderThickness
        {
            get { return (Thickness)GetValue(CoverageBarBorderThicknessProperty); }
            set { SetValue(CoverageBarBorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty CoverageBarBorderThicknessProperty =
            DependencyProperty.Register(nameof(CoverageBarBorderThickness), typeof(Thickness), typeof(CoveragePercentageBar), new PropertyMetadata(new Thickness(0), CoverageBarBorderThicknessChanged));

        private static void CoverageBarBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CoveragePercentageBar).CoverageBarBorderThicknessChanged();
        }

        private void CoverageBarBorderThicknessChanged()
        {
            if (ThemedBackgroundColor == Colors.Transparent)
            {
                CoverageBorderThicknessActual = CoverageBarBorderThickness;
            }
        }

        private Thickness CoverageBorderThicknessActual
        {
            get { return (Thickness)GetValue(CoverageBorderThicknessActualProperty); }
            set { SetValue(CoverageBorderThicknessActualProperty, value); }
        }

        private static readonly DependencyProperty CoverageBorderThicknessActualProperty =
            DependencyProperty.Register(nameof(CoverageBorderThicknessActual), typeof(Thickness), typeof(CoveragePercentageBar), new PropertyMetadata(new Thickness(0)));

        public Brush CoverageBorderBrush
        {
            get { return (Brush)GetValue(CoverageBorderBrushProperty); }
            set { SetValue(CoverageBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty CoverageBorderBrushProperty =
            DependencyProperty.Register(nameof(CoverageBorderBrush), typeof(Brush), typeof(CoveragePercentageBar), new PropertyMetadata(Brushes.Transparent));

        public Color CoveredColor
        {
            get { return (Color)GetValue(CoveredColorProperty); }
            set { SetValue(CoveredColorProperty, value); }
        }

        public static readonly DependencyProperty CoveredColorProperty =
            DependencyProperty.Register(nameof(CoveredColor), typeof(Color), typeof(CoveragePercentageBar), new PropertyMetadata(VisualStudioNotificationColors.Green, CoveredColorChanged));

        private static void CoveredColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CoveragePercentageBar).CoveredColorChanged();
        }
        private void CoveredColorChanged()
        {
            UpdateBrush(true);
        }
        public Color NotCoveredColor
        {
            get { return (Color)GetValue(NotCoveredColorProperty); }
            set { SetValue(NotCoveredColorProperty, value); }
        }

        public static readonly DependencyProperty NotCoveredColorProperty =
            DependencyProperty.Register(nameof(NotCoveredColor), typeof(Color), typeof(CoveragePercentageBar), new PropertyMetadata(VisualStudioNotificationColors.Red, NotCoveredColorChanged));

        private static void NotCoveredColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CoveragePercentageBar).NotCoveredColorChanged();
        }
        private void NotCoveredColorChanged()
        {
            UpdateBrush(false);
        }

        private Brush CoveredBrush
        {
            get { return (Brush)GetValue(CoveredBrushProperty); }
            set { SetValue(CoveredBrushProperty, value); }
        }

        private static readonly DependencyProperty CoveredBrushProperty =
            DependencyProperty.Register(nameof(CoveredBrush), typeof(Brush), typeof(CoveragePercentageBar), new PropertyMetadata(new SolidColorBrush(VisualStudioNotificationColors.Green)));

        private Brush NotCoveredBrush
        {
            get { return (Brush)GetValue(NotCoveredBrushProperty); }
            set { SetValue(NotCoveredBrushProperty, value); }
        }

        private static readonly DependencyProperty NotCoveredBrushProperty =
            DependencyProperty.Register(nameof(NotCoveredBrush), typeof(Brush), typeof(CoveragePercentageBar), new PropertyMetadata(new SolidColorBrush(VisualStudioNotificationColors.Red)));

        public event PropertyChangedEventHandler PropertyChanged;

        private void UpdateBrush(bool isCovered)
        {
            var baseColor = isCovered ? CoveredColor : NotCoveredColor;
            SolidColorBrush brush = null;
            if (ThemedBackgroundColor == Colors.Transparent)
            {
                brush = new SolidColorBrush(baseColor);
            }
            else
            {
                brush = ImageThemingUtilitiesX.ThemeColorToSolidBrush(baseColor, ThemedBackgroundColor);
            }
            brush.Freeze();

            if (isCovered)
                CoveredBrush = brush;
            else
                NotCoveredBrush = brush;
        }
    }
}
