using System;
using System.Linq;
using Avalonia.Data.Converters;
using System.Globalization;
using Avalonia.Data;

namespace UIClient.ViewModels;

public class TextCaseConverter : IValueConverter
{
    public static readonly TextCaseConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is int sourceNumber && targetType.IsAssignableTo(typeof(string)))
        {
            return sourceNumber.ToString();
        }
        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is string sourceText && targetType.IsAssignableTo(typeof(int)))
        {
            var filtered = string.Join("",sourceText.Where(c => Char.IsDigit(c)).Select(c => new string(c,1)));
            if(int.TryParse(filtered, out var actualValue))
            {
                return actualValue;
            }
        }
        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }
}
