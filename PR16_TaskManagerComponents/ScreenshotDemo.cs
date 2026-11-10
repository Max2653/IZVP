using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using TaskManager.Controls;
using TaskManager.Services;

namespace TaskManager;

/// <summary>
/// Сценарій із завдання: додати два завдання, змінити стан першого, видалити друге.
/// Дії виконуються через самі компоненти (позначка і кнопка картки), після кожного кроку — знімок вікна.
/// </summary>
internal static class ScreenshotDemo
{
    public static void Attach(MainWindow window, TaskList tasks, string folder)
    {
        window.Opened += async (_, _) =>
        {
            Directory.CreateDirectory(folder);

            foreach (var title in new[] { "Підготувати конспект", "Зробити практичну" })
            {
                window.Input.Text = title;
                window.AddFromInput();
            }
            await Task.Delay(600);
            Save(window, folder, "1_two_tasks.png");

            var cards = window.GetVisualDescendants().OfType<TaskItemControl>().ToList();
            cards[0].IsDone = true;            // як клік по позначці першої картки
            await Task.Delay(600);
            Save(window, folder, "2_first_done.png");

            cards[1].RequestDelete();          // як клік «Видалити» на другій картці
            await Task.Delay(600);
            Save(window, folder, "3_second_deleted.png");

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
