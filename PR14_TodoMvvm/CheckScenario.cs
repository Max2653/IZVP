using System.Text;
using TodoMvvm.ViewModels;
using TodoMvvm.Views;

namespace TodoMvvm;

/// <summary>
/// Сценарій перевірки з завдання: додати 2 завдання, змінити статус одного,
/// видалити друге, спробувати додати порожню назву. Дії виконуються через ті самі команди,
/// що й кнопки у вікні; після кожного кроку зберігається знімок і рядок у журналі check.txt.
/// </summary>
internal static class CheckScenario
{
    public static void Attach(MainForm view, TaskListViewModel vm, string folder)
    {
        Directory.CreateDirectory(folder);
        var log = new StringBuilder();

        view.Shown += async (_, _) =>
        {
            int step = 0;
            async Task Snap(string title, bool passed)
            {
                step++;
                await Task.Delay(300);
                string state = string.Join("; ", vm.Tasks.Select(t => t.ToString()));
                log.AppendLine($"{(passed ? "[OK]  " : "[FAIL]")} {step}. {title}");
                log.AppendLine($"       Список: {(state.Length == 0 ? "(порожній)" : state)}");
                log.AppendLine($"       Повідомлення: {vm.Message}");

                using var bmp = new Bitmap(view.Width, view.Height);
                view.DrawToBitmap(bmp, new Rectangle(Point.Empty, view.Size));
                using var ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                File.WriteAllBytes(Path.Combine(folder, $"{step}.png"), ms.ToArray());
            }

            // 1. Додавання двох завдань
            vm.NewTitle = "Підготувати звіт";
            vm.AddCommand.Execute(null);
            vm.NewTitle = "Купити продукти";
            vm.AddCommand.Execute(null);
            await Snap("Додано два завдання", vm.Tasks.Count == 2 && vm.Tasks.All(t => !t.IsDone));

            // 2. Зміна статусу першого
            vm.SelectedTask = vm.Tasks[0];
            vm.ToggleStatusCommand.Execute(null);
            await Snap("Статус «Підготувати звіт» змінено на «Виконано»", vm.Tasks[0].IsDone);

            // 3. Видалення другого
            vm.SelectedTask = vm.Tasks[1];
            vm.DeleteCommand.Execute(null);
            await Snap("Видалено «Купити продукти»", vm.Tasks.Count == 1 && vm.Tasks[0].Title == "Підготувати звіт");

            // 4. Спроба додати порожню назву
            vm.NewTitle = "   ";
            vm.AddCommand.Execute(null);
            await Snap("Порожню назву не додано, показано помилку", vm.Tasks.Count == 1 && vm.IsError);

            File.WriteAllText(Path.Combine(folder, "check.txt"), log.ToString(), Encoding.UTF8);
            view.Close();
        };
    }
}
