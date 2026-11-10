namespace OrderApp;

/// <summary>Вводить дані у форму як користувач, натискає «Розрахувати» і зберігає знімки вікна.</summary>
internal static class ScreenshotDemo
{
    public static void Attach(MainForm form, string folder)
    {
        Directory.CreateDirectory(folder);
        form.ShowMessageBoxes = false; // MessageBox заблокував би сценарій; помилка все одно видна у вікні

        var cases = new (string File, string Amount, string Discount)[]
        {
            ("1_normal.png", "1200", "10"),
            ("2_zero_discount.png", "1200", "0"),
            ("3_invalid_discount.png", "1200", "120"),
            ("4_negative_amount.png", "-50", "5")
        };

        form.Shown += async (_, _) =>
        {
            try
            {
                foreach (var c in cases)
                {
                    form.AmountBox.Text = c.Amount;
                    form.DiscountBox.Text = c.Discount;
                    form.CalculateButton.PerformClick();
                    await Task.Delay(300);

                    using var bmp = new Bitmap(form.Width, form.Height);
                    form.DrawToBitmap(bmp, new Rectangle(Point.Empty, form.Size));
                    // через MemoryStream: GDI+ не підтримує довгі шляхи до файлів
                    using var ms = new MemoryStream();
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    File.WriteAllBytes(Path.Combine(folder, c.File), ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(folder, "error.txt"), ex.ToString());
            }
            form.Close();
        };
    }
}
