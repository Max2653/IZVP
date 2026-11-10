using OrderCore;

// Виклик OrderService без жодного GUI — та сама логіка, що й у вікні.
Console.OutputEncoding = System.Text.Encoding.UTF8;

var service = new OrderService();
int failed = 0;

Check("Звичайний розрахунок (1200 грн, 10%)", service.Calculate("1200", "10"), expectSuccess: true, expectedTotal: 1080m);
Check("Нульова знижка (1200 грн, 0%)", service.Calculate("1200", "0"), expectSuccess: true, expectedTotal: 1200m);
Check("Некоректна знижка (1200 грн, 120%)", service.Calculate("1200", "120"), expectSuccess: false);
Check("Від'ємна сума (-50 грн, 5%)", service.Calculate("-50", "5"), expectSuccess: false);
Check("Нечислове значення (\"abc\", 10%)", service.Calculate("abc", "10"), expectSuccess: false);
Check("Дробові значення (99,90 грн, 15%)", service.Calculate("99,90", "15"), expectSuccess: true, expectedTotal: 84.91m);

Console.WriteLine();
Console.WriteLine(failed == 0 ? "Усі перевірки пройдено." : $"Не пройдено перевірок: {failed}");
return failed == 0 ? 0 : 1;

void Check(string title, OrderResult result, bool expectSuccess, decimal? expectedTotal = null)
{
    bool ok = result.Success == expectSuccess && (expectedTotal is null || result.Total == expectedTotal);
    if (!ok) failed++;

    string details = result.Success
        ? $"до сплати {result.Total:0.##} грн (знижка {result.DiscountValue:0.##} грн)"
        : $"помилка: {result.Error}";
    Console.WriteLine($"[{(ok ? "OK" : "FAIL")}] {title} → {details}");
}
