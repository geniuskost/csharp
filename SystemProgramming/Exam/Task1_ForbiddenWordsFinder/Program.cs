namespace ForbiddenWordsFinder;

internal static class Program
{
    private const string MutexName = @"Global\ForbiddenWordsFinder_Exam";

    [STAThread]
    private static void Main(string[] args)
    {
        var options = ApplicationOptions.Parse(args);

        using var mutex = new Mutex(true, MutexName, out var createdNew);
        if (!createdNew)
        {
            if (!options.Headless)
            {
                MessageBox.Show(
                    "Додаток уже запущено.",
                    "Forbidden Words Finder",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            return;
        }

        if (options.Headless)
        {
            ConsoleRunner.RunAsync(options).GetAwaiter().GetResult();
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(options));
    }
}
