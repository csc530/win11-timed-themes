// See https://aka.ms/new-console-template for more information

using static Timed_Theme.Clock;

namespace Timed_Theme;

internal class Config
{
	public static readonly string themesPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)+"\\Microsoft\\Windows\\Themes";

	// todo improve prompts to be entered on the same line, make it a claasss?
	public void Run(string[] args)
	{
		if (!isWin11())
			exit(ErrorCodes.ErrorNotSupported); //50


		var themes = getThemes();

		if (getMode() == 1)
		{
			Console.WriteLine("Time mode selected");
			var nightTime = setTime();
			Console.WriteLine("Night time set to {0}", nightTime);
			Console.WriteLine();

			var dayTime = setTime();
			Console.WriteLine("Day time set to {0}", dayTime);
			Console.WriteLine();


			Console.WriteLine("Select which theme to be used for night (mode): ");
			var nightTheme = getThemeInput(themes);
			Console.WriteLine();
			Console.WriteLine("Select which theme to be used for day (mode): ");

			var dayTheme = getThemeInput(themes);
			saveTheme(nightTheme, nightTime, dayTheme, dayTime);
			Console.WriteLine("Theme settings saved.");
			Console.WriteLine();
			exit(ErrorCodes.None);
		}

		else
		{
			Console.WriteLine("Event mode selected");
		}
	}



	#region Functions
	private bool saveTheme(string nightTheme, TimeOnly nightTime, string dayTheme, TimeOnly dayTime)
	{
		var settings = File.CreateText($"{themesPath}\\theme.csc");
		settings.WriteLine($"night={nightTheme}");
		settings.WriteLine($"night-time={nightTime.ToString("HH:mm")}");
		settings.WriteLine($"day={dayTheme}");
		settings.WriteLine($"day-time={dayTime.ToString("HH:mm")}");
		settings.Flush();
		settings.Close();
		return true;
	}
	private int exit(ErrorCodes err)
	{
		var code = (int)err;
		Environment.Exit(code);
		return code;
	}

	private int getMode()
	{
		Console.WriteLine("Would you like to change theme by time or event?");
		Console.WriteLine("1. Time");
		Console.WriteLine("2. Event");
		Console.Write("Enter your choice: ");
		var choice = Console.ReadLine();
		if (choice == null) return exit(ErrorCodes.ErrorBadCommand);

		if (choice == "1" || choice.Equals("time", StringComparison.OrdinalIgnoreCase) ||
		    choice.Equals("t", StringComparison.OrdinalIgnoreCase))
			return 1;

		if (choice == "2" || choice.Equals("event", StringComparison.OrdinalIgnoreCase) ||
		    choice.Equals("e", StringComparison.OrdinalIgnoreCase))
		{
			return 2;
		}

		Console.WriteLine("Invalid choice, try again");
		// * pretty comfortable recursing as it will be a stuupid # of times before a stackoverflows occurs
		return getMode();
	}

	private bool isWin11()
	{
		var os = Environment.OSVersion;
		var pid = os.Platform;
		var ver = os.Version.Major;
		//print all vars in one line
		Console.WriteLine("OS: {0}\nPlatform: {1}\nVersion: {2}", os, pid, ver);
		//check for Windows 10+
		if (pid == PlatformID.Win32NT && ver >= 10)
		{
			Console.WriteLine("Windows 10 or higher detected");
			return true;
		}

		Console.WriteLine("Windows 10 or higher not detected");
		Console.WriteLine("This program only works on Windows 10 or higher");
		return false;
	}


	private List<string> getThemes()
	{
		var files = Directory.GetFiles(themesPath);
		var dir = Directory.GetDirectories(themesPath);
		var list = files.ToList();
		list.AddRange(dir);
		var themes = new List<string>();
		list.ForEach(path =>
		{
			var pathList = path.Split("\\", StringSplitOptions.RemoveEmptyEntries);
			var name = pathList[^1].Split(".", 1)[0];
			themes.Add(name);
		});
		return themes;
	}

	private string getThemeInput(List<string> themes)
	{
		var i = 1;
		themes.ForEach(theme => Console.WriteLine($"({i++}). {theme}"));
		Console.WriteLine();
		Console.Write("Select a theme by entering its numbered position: ");
		var select = Console.ReadLine();
		var isNum = int.TryParse(select, out var index);
		if (isNum && themes.Count > --index)
		{
			Console.Write($"Confirm {themes[index]} as night theme? (y/N): ");
			var correct = Console.ReadLine();
			Console.WriteLine();
			if (correct != null && correct.Equals("y", StringComparison.OrdinalIgnoreCase))
				return themes[index];
			return getThemeInput(themes);
		}

		Console.WriteLine("Invalid number, out of range input number between 1 - {0}", themes.Count);
		Console.WriteLine();
		return getThemeInput(themes);
	}

	private TimeOnly setTime()
	{
		Console.WriteLine("Enter the time you want to change to night mode (hh:mm:ss): ");
		var night = inputTime();
		Console.WriteLine(
			$"Is {night.ToString("HH:mm")} ({night}) the correct time for dark mode to be activated? (y/N): ");
		var correct = Console.ReadLine();
		if (correct != null && correct.Equals("y", StringComparison.OrdinalIgnoreCase))
			return night;
		return setTime();
	}

	private void toggleTime(object? sender, ChangeEventArgs e)
	{
		var time = e.newTime.ToString("HH:mm");
		Console.CursorLeft = Console.WindowWidth / 2 - time.Length;
		Console.Write(time);
		Console.CursorLeft -= time.Length - 2;
	}

	private TimeOnly inputTime()
	{
		Console.ForegroundColor = ConsoleColor.Cyan;
		var clock = new Clock();
		EventHandler<ChangeEventArgs> changeConsoleTime = toggleTime;
		clock.OnChangeHandler += changeConsoleTime;

		var key = new ConsoleKeyInfo();
		while (key.Key != ConsoleKey.Enter)
		{
			switch (key.Key)
			{
				case ConsoleKey.UpArrow:
					clock.increment(1, ClockUnit.Minutes);
					break;
				case ConsoleKey.DownArrow:
					clock.increment(-1, ClockUnit.Minutes);
					break;
				case ConsoleKey.LeftArrow:
					clock.increment(-1, ClockUnit.Hours);
					break;
				case ConsoleKey.RightArrow:
					clock.increment(1, ClockUnit.Hours);
					break;
				default:
					Console.ForegroundColor = ConsoleColor.Red;
					Console.CursorTop--;
					Console.CursorLeft = 0;
					Console.WriteLine(
						"Please use the up and down arrows to adjust the hour, and the left and right arrows to adjust the minute.");
					Console.ForegroundColor = ConsoleColor.Cyan;
					break;
			}

			key = Console.ReadKey(true);
		}

		Console.WriteLine();
		Console.ResetColor();
		return clock.Time;
	}

	#endregion
}