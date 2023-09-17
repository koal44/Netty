﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Netryoshka
{
    public class DynamicResourceBindingExtension : MarkupExtension
    {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DynamicResourceBindingExtension() { }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DynamicResourceBindingExtension(object resourceKey)
            => ResourceKey = resourceKey ?? throw new ArgumentNullException(nameof(resourceKey));

        public object ResourceKey { get; set; }
        public IValueConverter Converter { get; set; }
        public object ConverterParameter { get; set; }
        public CultureInfo ConverterCulture { get; set; }
        public string StringFormat { get; set; }
        public object TargetNullValue { get; set; }

        private BindingProxy bindingProxy;
        private BindingTrigger bindingTrigger;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {

            // Create the BindingProxy for the requested dynamic resource
            // This will be used as the source of the underlying binding
            var dynamicResource = new DynamicResourceExtension(ResourceKey);
            bindingProxy = new BindingProxy(dynamicResource.ProvideValue(null)); // Pass 'null' here

            // Set up the actual, underlying binding specifying the just-created
            // BindingProxy as its source. Note, we don't yet set the Converter,
            // ConverterParameter, StringFormat or TargetNullValue (More on why not below)
            var dynamicResourceBinding = new Binding()
            {
                Source = bindingProxy,
                Path = new PropertyPath(BindingProxy.ValueProperty),
                Mode = BindingMode.OneWay
            };

            // Get the TargetInfo for this markup extension
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            var targetInfo = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            // Check if the target object of this markup extension is a DependencyObject.
            // If so, we can set up everything right now and we're done!
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            if (targetInfo.TargetObject is DependencyObject dependencyObject)
            {

                // Ok, since we're being applied directly on a DependencyObject, we can
                // go ahead and set all the additional binding-related properties.
                dynamicResourceBinding.Converter = Converter;
                dynamicResourceBinding.ConverterParameter = ConverterParameter;
                dynamicResourceBinding.ConverterCulture = ConverterCulture;
                dynamicResourceBinding.StringFormat = StringFormat;
                dynamicResourceBinding.TargetNullValue = TargetNullValue;

                // If the DependencyObject is a FrameworkElement, then we also add the
                // BindingProxy to its Resources collection to ensure proper resource lookup
                // We use itself as it's key so we can check for it's existence later
                if (dependencyObject is FrameworkElement targetFrameworkElement)
                    targetFrameworkElement.Resources[bindingProxy] = bindingProxy;

                // And now we simply return the same value as the actual, underlying binding,
                // making us mimic being a proper binding, hence the markup extension's name
                return dynamicResourceBinding.ProvideValue(serviceProvider);
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            // Ok, we're not being set directly on a DependencyObject. Most likely we're being set via
            // a style so we need to do some extra work to get the ultimate target of the binding.
            //
            // We do this by setting up a wrapper MultiBinding, where we add the above binding
            // as well as a second child binding with a RelativeResource of 'Self'. During the
            // Convert method, we use this to get the ultimate/actual binding target.
            //
            // Finally, since we have no way of getting the BindingExpression (as there will be a
            // separate one for each case where this style is ultimately applied), we create a third
            // binding whose only purpose is to manually re-trigger the execution of the 'WrapperConvert' 
            // method, allowing us to discover the ultimate target via the second child binding above.

            // Binding used to find the target this markup extension is ultimately applied to
            var findTargetBinding = new Binding()
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            };

            // Binding used to manually 'retrigger' the WrapperConvert method. (See BindingTrigger's implementation)
            bindingTrigger = new BindingTrigger();

            // Wrapper binding to bring everything together
            var wrapperBinding = new MultiBinding()
            {
                Bindings = {
                dynamicResourceBinding,
                findTargetBinding,
                bindingTrigger.Binding
            },
                Converter = new InlineMultiConverter(WrapperConvert)
            };

            // Just like above, we return the result of the wrapperBinding's ProvideValue
            // call, again making us mimic the behavior of being an actual binding
            return wrapperBinding.ProvideValue(serviceProvider);
        }

        // This gets called on every change of the dynamic resource, for every object this
        // markup extension has been applied to, whether applied directly, or via a style
        private object WrapperConvert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            var dynamicResourceBindingResult = values[0]; // This is the result of the DynamicResourceBinding**
            var bindingTargetObject = values[1]; // This is the ultimate target of the binding

            // ** Note: This value has not yet been passed through the converter, nor been coalesced
            // against TargetNullValue, or, if applicable, formatted, all of which we have to do below.

            // We can ignore the third value (i.e. 'values[2]') as that's the result of the bindingTrigger's
            // binding, which will always be set to null (See BindingTrigger's implementation for more info)
            // Again that binding only exists to re-trigger this WrapperConvert method explicitly when needed.

            if (Converter != null)
                // We pass in the TargetType we're handed in this method as that's the real binding target.
                // Normally, child bindings would been handed 'object' since their target is the MultiBinding.
                dynamicResourceBindingResult = Converter.Convert(dynamicResourceBindingResult, targetType, ConverterParameter, ConverterCulture);

            // First, check the results for null. If so, set it equal to TargetNullValue and continue
            if (dynamicResourceBindingResult == null)
                dynamicResourceBindingResult = TargetNullValue;

            // It's not null, so check both a) if the target type is a string, and b) that there's a
            // StringFormat. If both are true, format the string accordingly.
            //
            // Note: You can't simply put those properties on the MultiBinding as it handles things
            // differently than a regular Binding (e.g. StringFormat is always applied, even when null.)
            else if (targetType == typeof(string) && StringFormat != null)
                dynamicResourceBindingResult = String.Format(StringFormat, dynamicResourceBindingResult);

            // If the binding target object is a FrameworkElement, ensure the binding proxy is added
            // to its Resources collection so it will be part of the lookup relative to that element
            if (bindingTargetObject is FrameworkElement targetFrameworkElement
            && !targetFrameworkElement.Resources.Contains(bindingProxy))
            {

                // Add the resource to the target object's Resources collection
                targetFrameworkElement.Resources[bindingProxy] = bindingProxy;

                // Since we just added the binding proxy to the visual tree, we have to re-evaluate it
                // relative to where we now are.  However, since there's no way to get a BindingExpression
                // to manually refresh it from here, here's where the BindingTrigger created above comes
                // into play.  By manually forcing a change notification on it's Value property, it will
                // retrigger the binding for us, achieving the same thing.  However...
                //
                // Since we're presently executing in the WrapperConvert method from the current binding
                // operation, we must retrigger that refresh to occur *after* this execution completes. We
                // can do this by putting the refresh code in a closure passed to the 'Post' method on the
                // current SynchronizationContext. This schedules that closure to run in the future, as part
                // of the normal run-loop cycle. If we didn't schedule this in this way, the results will be
                // returned out of order and the UI wouldn't update properly, overwriting the actual values.

                // Refresh the binding, but not now, in the future
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                SynchronizationContext.Current.Post((state) => {
                    bindingTrigger.Refresh();
                }, null);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }

            // Return the now-properly-resolved result of the child binding
            return dynamicResourceBindingResult;
        }
    }


    public class BindingProxy : Freezable
    {

        public BindingProxy() { }
        public BindingProxy(object value)
            => Value = value;

        protected override Freezable CreateInstanceCore()
            => new BindingProxy();

        #region Value Property

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(object),
            typeof(BindingProxy),
            new FrameworkPropertyMetadata(default));

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        #endregion Value Property
    }

    public class BindingTrigger : INotifyPropertyChanged
    {
        public BindingTrigger()
            => Binding = new Binding()
            {
                Source = this,
                Path = new PropertyPath(nameof(Value))
            };

        public event PropertyChangedEventHandler? PropertyChanged;

        public Binding Binding { get; }

        public void Refresh()
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));

        public object Value { get; }
    }


    public class InlineMultiConverter : IMultiValueConverter
    {

        public delegate object ConvertDelegate(object[] values, Type targetType, object parameter, CultureInfo culture);
        public delegate object[] ConvertBackDelegate(object value, Type[] targetTypes, object parameter, CultureInfo culture);

        public InlineMultiConverter(ConvertDelegate convert, ConvertBackDelegate? convertBack = null)

        {
            _convert = convert ?? throw new ArgumentNullException(nameof(convert));
            _convertBack = convertBack;
        }

        private readonly ConvertDelegate _convert;
        private readonly ConvertBackDelegate? _convertBack;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => _convert(values, targetType, parameter, culture);

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => (_convertBack != null)
                ? _convertBack(value, targetTypes, parameter, culture)
                : throw new NotImplementedException("InlineMultiConverter.ConvertBack()");
    }

}



