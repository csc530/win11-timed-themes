using Timed_Theme;

string themesPath = Config.ThemesPath;
var conf = new Config();
var themes =conf.ThemeConfigurations;
conf.ThemeConfigurations.SetCurrentTheme();
try
{
	var config = new FileSystemWatcher($"{themesPath}", Config.ConfigFilePath);
	config.EnableRaisingEvents = true;
	config.Changed += (sender, e) =>
	{
		themes = conf.ThemeConfigurations;
		Console.WriteLine("Changed\n\n"+themes);
		conf.ThemeConfigurations.SetCurrentTheme();
	};
	config.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
	Console.WriteLine("Watching " + config.Filter + " file(s) in " + config.Path);


	config.Changed += (object sender, FileSystemEventArgs e) =>
	{
		if(e.Name != "config.csc")
			Rename(e);
		else if(e.Name == "config.csc")
			reconfig();
	};

	void Rename(FileSystemEventArgs e)
	{
		File.Copy(e.FullPath, Config.ConfigFilePath);
		File.Delete(e.FullPath);
	}

	void reconfig()
	{
	}

	void switchTheme()
	{
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