using System.Text.RegularExpressions;

namespace HospitalApp;

public static class ValidationHelper
{
    public static void ValidateColumn(ColumnConfig column, object? value, bool skipTomorrowRule = false)
    {
        if (value == DBNull.Value)
            value = null;

        if (value == null)
        {
            if (column.IsRequired)
                throw new InvalidOperationException($"Поле \"{column.Label}\" є обов'язковим.");

            return;
        }

        switch (column.Kind)
        {
            case FieldKind.Text:
                ValidateText(column.Label, value.ToString(), column.IsRequired, column.MinLength, column.MaxLength, column.RegexPattern, column.RegexErrorMessage);
                break;
            case FieldKind.Integer:
                ValidateInteger(column.Label, Convert.ToInt32(value), column.MinValue, column.MaxValue);
                break;
            case FieldKind.Date:
                ValidateDate(column.Label, Convert.ToDateTime(value).Date, column.MinDate, column.MaxDateIsToday, column.MinDateIsTomorrow && !skipTomorrowRule);
                break;
            case FieldKind.ForeignKey:
                if (column.IsRequired && value == null)
                    throw new InvalidOperationException($"Поле \"{column.Label}\" є обов'язковим.");
                break;
        }
    }

    public static void ValidateText(
        string label,
        string? value,
        bool required,
        int? minLength,
        int? maxLength,
        string? regexPattern = null,
        string? regexErrorMessage = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required)
                throw new InvalidOperationException($"Поле \"{label}\" є обов'язковим.");

            return;
        }

        var length = value.Length;
        if (minLength.HasValue && maxLength.HasValue && (length < minLength.Value || length > maxLength.Value))
            throw new InvalidOperationException($"Поле \"{label}\" має містити від {minLength.Value} до {maxLength.Value} символів.");

        if (minLength.HasValue && length < minLength.Value)
            throw new InvalidOperationException($"Поле \"{label}\" має містити щонайменше {minLength.Value} символів.");

        if (maxLength.HasValue && length > maxLength.Value)
            throw new InvalidOperationException($"Поле \"{label}\" має містити не більше {maxLength.Value} символів.");

        if (!string.IsNullOrWhiteSpace(regexPattern) && !Regex.IsMatch(value, regexPattern))
            throw new InvalidOperationException(regexErrorMessage ?? $"Поле \"{label}\" має неправильний формат.");
    }

    public static void ValidateInteger(string label, int value, int? minValue, int? maxValue)
    {
        if (minValue.HasValue && maxValue.HasValue && (value < minValue.Value || value > maxValue.Value))
            throw new InvalidOperationException($"Поле \"{label}\" має бути в діапазоні від {minValue.Value} до {maxValue.Value}.");

        if (minValue.HasValue && value < minValue.Value)
            throw new InvalidOperationException($"Поле \"{label}\" має бути не менше {minValue.Value}.");

        if (maxValue.HasValue && value > maxValue.Value)
            throw new InvalidOperationException($"Поле \"{label}\" має бути не більше {maxValue.Value}.");
    }

    public static void ValidateDate(string label, DateTime value, DateTime? minDate, bool maxDateIsToday, bool minDateIsTomorrow)
    {
        if (minDate.HasValue && value.Date < minDate.Value.Date)
            throw new InvalidOperationException($"Поле \"{label}\" має бути не раніше {minDate.Value:dd.MM.yyyy}.");

        if (maxDateIsToday && value.Date > DateTime.Today)
            throw new InvalidOperationException($"{label} не може бути пізнішою за сьогодні.");

        if (minDateIsTomorrow && value.Date < DateTime.Today.AddDays(1))
            throw new InvalidOperationException($"{label} має бути не раніше завтрашнього дня.");
    }
}
