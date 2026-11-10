namespace PR12_DataBinding;

/// <summary>
/// Допоміжний режим для звіту: проходить три стани програми і зберігає знімки вікна.
/// 1_initial.png — початковий стан;
/// 2_changed.png — після зміни даних (зміна моделі з коду + редагування через поле);
/// 3_invalid.png — результат перевірки некоректного введення.
/// </summary>
internal static class ScreenshotDemo
{
    public static void Attach(MainForm form, string folder)
    {
        Directory.CreateDirectory(folder);
        form.Shown += async (_, _) =>
        {
            await Task.Delay(400);
            Save(form, folder, "1_initial.png");

            // зміна даних у моделі — UI оновлюється автоматично
            if (form.Source.Current is Student s)
            {
                s.Name = "Іваненко Олена Петрівна";
                s.Grade = 93;
            }
            await Task.Delay(300);
            Save(form, folder, "2_changed.png");

            // некоректне введення через інтерфейс
            form.GradeBox.Focus();
            form.GradeBox.Text = "abc";
            form.ValidateChildren();
            await Task.Delay(300);
            Save(form, folder, "3_invalid.png");

            form.Close();
        };
    }

    private static void Save(Form form, string folder, string file)
    {
        using var bmp = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(bmp, new Rectangle(Point.Empty, form.Size));
        bmp.Save(Path.Combine(folder, file), System.Drawing.Imaging.ImageFormat.Png);
    }
}
