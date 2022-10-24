using Timed_Theme;
using static Timed_Theme.ThemeSchedule;

string themesPath = Config.ThemesPath;
var conf = new Config();
conf.ThemeConfigurations.SetCurrentTheme();
var clock = new Clock();
try
{
    var configWatcher = new FileSystemWatcher($"{themesPath}", Config.ConfigFilePath);
    configWatcher.EnableRaisingEvents = true;
    configWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
    Console.WriteLine("Watching " + configWatcher.Filter + " file(s) in " + configWatcher.Path);
    configWatcher.Changed += UpdateConfiguration;
    configWatcher.Renamed += UpdateConfiguration;
    Wait();

    void Wait()
    {
        clock.start();

        var input = Console.ReadLine();
        while(true)
            if(input == "exit")
                break;
            else if(input == "config" || input == "settings" || input == "edit" || input == "configurations")
            {
                conf.Configure();
                input = Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Type 'exit' to exit");
                Console.WriteLine("'config' to change current settings");
                var next = (conf.ThemeConfigurations.NextTheme());
                Console.WriteLine($"Next theme starts at {next.Key}");
                Console.WriteLine("time until switch " + (next.Key - CurrentTime));
                Console.WriteLine($"Current time = {clock.ToLongString()}");
                input = Console.ReadLine();
            }
    }

    void UpdateConfiguration(object sender, FileSystemEventArgs e)
    {
        if(e.Name != "config.csc")
        {
            File.Copy(e.FullPath, Config.ConfigFilePath);
            File.Delete(e.FullPath);
        }
        else if(e.Name == "config.csc")
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
        catch(Exception)
        {
            // Something went wrong
        }
    }
}
catch(ArgumentException e)
{
    Console.Error.WriteLine(e);
}