using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using TaskManager.Services;

namespace TaskManager;

/// <summary>Демонстраційний сценарій для звіту: додає завдання, змінює статус, видаляє і зберігає знімки вікна.</summary>
internal static class ScreenshotDemo
{
    public static void Attach(MainWindow window, TaskList tasks, string folder)
    {
        window.Opened += async (_, _) =>
        {
            Directory.CreateDirectory(folder);

            foreach (var title in new[] { "Підготувати практичну роботу", "Здати звіт з ІЗВП", "Прочитати лекцію 14" })
            {
                window.Input.Text = title;
                window.AddFromInput();
            }
            tasks.Items[0].IsDone = true;      // позначка «Виконано»
            window.List.SelectedItem = tasks.Items[1];
            await Task.Delay(600);
            Save(window, folder, "1_tasks.png");

            window.DeleteSelected();           // видалення вибраного
            window.Input.Text = "   ";
            window.AddFromInput();             // перевірка порожнього введення
            await Task.Delay(600);
            Save(window, folder, "2_after_delete_and_empty_input.png");

            window.Close();
        };
    }

    private static void Save(Window window, string folder, string file)
    {
        var size = new PixelSize((int)window.Bounds.Width, (int)window.Bounds.Height);
        using var bitmap = new RenderTargetBitmap(size, new Vector(96, 96));
        bitmap.Render(window);
        using var ms = new MemoryStream();
        bitmap.Save(ms);
        File.WriteAllBytes(Path.Combine(folder, file), ms.ToArray());
    }
}
