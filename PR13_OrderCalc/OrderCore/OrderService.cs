using System.Globalization;

namespace OrderCore;

/// <summary>Результат розрахунку: або успіх із сумами, або повідомлення про помилку.</summary>
public sealed record OrderResult(bool Success, decimal Amount, decimal DiscountPercent, decimal DiscountValue, decimal Total, string? Error)
{
    public static OrderResult Ok(decimal amount, decimal percent, decimal discount, decimal total) =>
        new(true, amount, percent, discount, total, null);

    public static OrderResult Fail(string error) =>
        new(false, 0, 0, 0, 0, error);
}

/// <summary>
/// Бізнес-логіка «Розрахунку замовлення»: перевірка введених даних і обчислення суми до сплати.
/// Клас нічого не знає про Windows Forms — його можна викликати з GUI, консолі чи тестів.
/// </summary>
public class OrderService
{
    /// <summary>Розрахунок із текстових значень, як їх ввів користувач.</summary>
    public OrderResult Calculate(string? amountText, string? discountText)
    {
        if (!TryParseDecimal(amountText, out decimal amount))
            return OrderResult.Fail("Сума має бути числом (наприклад, 1200 або 99,50).");

        if (string.IsNullOrWhiteSpace(discountText))
            discountText = "0"; // порожнє поле знижки = 0%

        if (!TryParseDecimal(discountText, out decimal discount))
            return OrderResult.Fail("Знижка має бути числом від 0 до 100.");

        return Calculate(amount, discount);
    }

    /// <summary>Розрахунок для вже числових значень.</summary>
    public OrderResult Calculate(decimal amount, decimal discountPercent)
    {
        if (amount < 0)
            return OrderResult.Fail("Сума покупки не може бути від'ємною.");

        if (discountPercent is < 0 or > 100)
            return OrderResult.Fail($"Неправильне значення знижки: {discountPercent}%. Допустимо від 0% до 100%.");

        decimal discountValue = Math.Round(amount * discountPercent / 100m, 2, MidpointRounding.AwayFromZero);
        decimal total = amount - discountValue;

        return OrderResult.Ok(amount, discountPercent, discountValue, total);
    }

    /// <summary>Приймає і кому, і крапку як десятковий роздільник.</summary>
    private static bool TryParseDecimal(string? text, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        string normalized = text.Trim().Replace(" ", "").Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }
}
