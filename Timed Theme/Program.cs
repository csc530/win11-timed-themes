using Timed_Theme;
using static Timed_Theme.ThemeSchedule;

string themesPath = Config.ThemesPath;
var conf = new Config();
conf.ThemeConfigurations.SetCurrentTheme();
try
{
    var config = new FileSystemWatcher($"{themesPath}", Config.ConfigFilePath);
    config.EnableRaisingEvents = true;
    config.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
    Console.WriteLine("Watching " + config.Filter + " file(s) in " + config.Path);
    config.Changed += UpdateConfiguration;
    config.Renamed += UpdateConfiguration;
    Wait();

    void Wait()
    {
        while (true)
            if (Console.ReadLine() == "exit")
                break;
            else
            {
                Console.WriteLine("Type 'exit' to exit");
               var next =(conf.ThemeConfigurations.NextTheme());
                Console.WriteLine($"Next theme starts at {next.Key}");
                Console.WriteLine("time until switch " + (next.Key-CurrentTime));
            }
    }

    void UpdateConfiguration(object sender, FileSystemEventArgs e)
    {
        if (e.Name != "config.csc")
        {
            File.Copy(e.FullPath, Config.ConfigFilePath);
            File.Delete(e.FullPath);
        }
        else if (e.Name == "config.csc")
            conf.Refresh();
        conf.ThemeConfigurations.SetCurrentTheme();
    }

    async Task ScheduleAction(Action action, TimeOnly executionTime)
    {
        try
        {
            await Task.Delay(TimeOnly.FromDateTime(DateTime.Now) - executionTime);
            action();
        }
        catch (Exception)
        {
            // Something went wrong
        }
    }
}
catch (ArgumentException e)
{
    Console.Error.WriteLine(e);
}