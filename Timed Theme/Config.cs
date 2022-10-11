// See https://aka.ms/new-console-template for more information

using static Timed_Theme.Clock;

namespace Timed_Theme;

internal class Config
{
	public static readonly string ThemesPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Microsoft\\Windows\\Themes";
	public static readonly string ConfigFilePath = ThemesPath + "\\config.csc";
	public List<ThemeSchedule> ThemeConfigurations { get; private set; } = new();

	// todo improve prompts to be entered on the same line, make it a claasss?
	/// <summary>
	/// Runs the configuration prompts to setup times and themes for the app
	/// </summary>
	public void Configurate()
	{
		if(!IsWin11())
			Exit(ErrorCodes.ErrorNotSupported);

		// todo: make it lazy load ie Task
		var themes = GetUsersThemes();

		if(true)
		{

			// night time and duration
			Console.WriteLine("Time mode selected");
			var nightTime = SetTime();
			Console.WriteLine("Night time set to {0}", nightTime);
			Console.WriteLine();
			// day time and duration
			var dayTime = SetTime();
			Console.WriteLine("Day time set to {0}", dayTime);
			Console.WriteLine();

			// night theme
			Console.WriteLine("Select which theme to be used for night (mode): ");
			var nightTheme = GetThemeInput(themes);
			Console.WriteLine();
			Console.WriteLine("Select which theme to be used for day (mode): ");
			// day theme
			var dayTheme = GetThemeInput(themes);
			SaveTheme(nightTheme, nightTime, dayTheme, dayTime);
			Console.WriteLine("Theme settings saved.");
			Console.WriteLine();
			Exit(ErrorCodes.None);
		}
		else
		{
			Console.WriteLine("Event mode selected");
		}
	}

	/// <summary>
	/// Saves the theme with the LocalApplication data of the users windows themes
	/// </summary>
	/// <param name="nightTheme">The night theme.</param>
	/// <param name="nightTime">The time to switch to the night theme</param>
	/// <param name="dayTheme">The day theme.</param>
	/// <param name="dayTime">The day time switch to the day theme.</param>
	/// <returns>If the config file was successfully saved to the system</returns>
	private static bool SaveTheme(string nightTheme, TimeOnly nightTime, string dayTheme, TimeOnly dayTime)
	{
		try
		{
			var settings = File.CreateText($"{ThemesPath}\\theme.csc");
			settings.WriteLine($"night={nightTheme}");
			settings.WriteLine($"night-time={nightTime.ToString("HH:mm")}");
			settings.WriteLine($"day={dayTheme}");
			settings.WriteLine($"day-time={dayTime.ToString("HH:mm")}");
			settings.Flush();
			settings.Close();
			return true;
		}
		catch(UnauthorizedAccessException e)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine("Could not save settings. Please run the program as administrator.");
			Console.ResetColor();
			Console.Error.WriteLine(e);
			return false;
		}
		catch(PathTooLongException e)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine("Could not save settings. File path too long.");
			Console.ResetColor();
			Console.Error.WriteLine(e);
			return false;
		}
		catch(Exception e)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine("Error: Could not save settings.");
			Console.ResetColor();
			Console.Error.WriteLine(e);
			return false;
		}
	}

	//todo: replace task with sync parent task and just cancel so it goes back to the main method
	private static int Exit(ErrorCodes err)
	{
		var code = (int)err;
		Environment.Exit(code);
		return code;
	}

	private int GetThemeSwitchMode()
	{
		Console.WriteLine("Would you like to change theme by time or event?");
		Console.WriteLine("1. Time");
		Console.WriteLine("2. Event");
		Console.Write("Enter your choice: ");

		var choice = Console.ReadLine();
		if(choice == null)
			return Exit(ErrorCodes.ErrorBadCommand);
		else if(choice == "1" || choice.Equals("time", StringComparison.OrdinalIgnoreCase) ||
			choice.Equals("t", StringComparison.OrdinalIgnoreCase))
			return 1;

		else if(choice == "2" || choice.Equals("event", StringComparison.OrdinalIgnoreCase) ||
			choice.Equals("e", StringComparison.OrdinalIgnoreCase))
		{
			return 2;
		}
		else
		{
			Console.WriteLine("Invalid choice, try again");
			// * pretty comfortable recursing as it will be a stuupid # of times before a stackoverflows occurs
			return GetThemeSwitchMode();
		}
	}

	/// <summary>
	/// Check if the system is compatible with the AutoThemeSwitcher app
	/// </summary>
	/// <returns>If the system is running Windows 10 or higher</returns>
	private static bool IsWin11()
	{
		var os = Environment.OSVersion;
		var pid = os.Platform;
		var ver = os.Version.Major;

		//check for Windows 10+
		if(pid == PlatformID.Win32NT && ver >= 10)
		{
			Console.WriteLine("App compatible: Windows 10 or higher detected");
			return true;
		}
		else
		{
			Console.WriteLine("Windows 10 or higher not detected");
			Console.WriteLine("This program only works on Windows 10 or higher");
			return false;
		}
	}


	/// <summary>
	/// Gets the user's themes.
	/// </summary>
	/// <returns>A list of the themes' names.</returns>
	private static List<string> GetUsersThemes()
	{
		var files = Directory.GetFiles(ThemesPath);
		var dir = Directory.GetDirectories(ThemesPath);
		var rawThemes = files.ToList();
		rawThemes.AddRange(dir);
		var themes = new List<string>();
		rawThemes.ForEach(path =>
		{
			var pathList = path.Split("\\", StringSplitOptions.RemoveEmptyEntries);
			var name = pathList[^1].Split(".", 1)[0];
			themes.Add(name);
		});
		return themes;
	}

	/// <summary>
	/// Gets the theme selection from user.
	/// </summary>
	/// <param name="themes">The themes to offer.</param>
	/// <returns>The selected theme.</returns>
	private string GetThemeInput(List<string> themes)
	{
		var i = 1;
		themes.ForEach(theme => Console.WriteLine($"{i++}). {theme}"));
		Console.WriteLine();
		Console.Write("Select a theme by entering its numbered position: ");
		var select = Console.ReadLine();
		var isNum = int.TryParse(select, out var index);
		if(isNum && themes.Count > --index)
		{
			//todo: add input for specific theme text and make a prompt/console wrinting class
			//! utilities include same line writing, prompt, alignments
			Console.Write($"Set {themes[index]} as theme? (y/N): ");
			var confirm = Console.ReadLine();
			Console.WriteLine();
			if(confirm != null && confirm.Equals("y", StringComparison.OrdinalIgnoreCase))
				return themes[index];
			else
				return GetThemeInput(themes);
		}
		else
		{
			Console.WriteLine("Invalid number, out of range input number between 1 - {0}", themes.Count);
			Console.WriteLine();
			return GetThemeInput(themes);
		}
	}

	/// <summary>
	/// Gets the time time to set for a theme from user.
	/// </summary>
	/// <returns>The selected time.</returns>
	private static TimeOnly SetTime()
	{
		Console.WriteLine("Enter the time you want to change to night mode (hh:mm:ss): ");
		var time = InputTime();
		Console.WriteLine($"Is {time.ToString("HH:mm")} ({time}) the correct time for dark mode to be activated? (y/N): ");
		var confirm = Console.ReadLine();
		if(confirm != null && confirm.Equals("y", StringComparison.OrdinalIgnoreCase))
			return time;
		return SetTime();
	}

	/// <summary>
	/// Console logic to change the displayed time.
	/// </summary>
	private static readonly EventHandler<ChangeEventArgs> ToggleTime = (object? sender, ChangeEventArgs e) =>
	{
		var time = e.newTime.ToString("HH:mm");
		//back to the beginning
		Console.CursorLeft = Console.WindowWidth / 2 - time.Length;
		Console.Write(time);
		//set cursor to pseudocenter for next input
		Console.CursorLeft -= time.Length - 2;
	};

	/// <summary>
	/// Gets the time from user input.
	/// </summary>
	/// <returns>The chosen time</returns>
	private static TimeOnly InputTime()
	{
		Console.ForegroundColor = ConsoleColor.Cyan;
		var clock = new Clock();
		clock.OnChangeHandler += ToggleTime;

		var key = new ConsoleKeyInfo();
		while(key.Key != ConsoleKey.Enter)
		{
			switch(key.Key)
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
					Console.WriteLine("Please use the up and down arrows to adjust the hour, and the left and right arrows to adjust the minute.");
					Console.ForegroundColor = ConsoleColor.Cyan;
					break;
			}
			key = Console.ReadKey(true);
		}
		Console.WriteLine();
		Console.ResetColor();
		return clock.Time;
	}

	/// <summary>
	/// Gets the paths to the user's themes.
	/// </summary>
	/// <returns>A list of the themes' paths.</returns>
	/// <remarks>Themes are stored in the user's AppData folder.</remarks>
	internal static List<string> GetUsersThemePaths()
	{
		var files = Directory.GetFiles(ThemesPath);
		var dir = Directory.GetDirectories(ThemesPath);
		var rawThemes = files.ToList();
		rawThemes.AddRange(dir);
		List<string> themes = new List<string>();
		Array.ForEach(dir, directory =>
		{
			var theme = new DirectoryInfo(directory).GetFiles("*.theme", SearchOption.TopDirectoryOnly);
			themes.Add(theme[0].FullName);
		});
		return themes as List<string>;
	}

	/// <summary>
	/// Gets current theme configurations
	/// </summary>
	/// <returns>Dictionary of each theme and their set times.</returns>
	/// <remarks>Themes are stored in the user's AppData folder.</remarks>
	internal static Dictionary<string, TimeOnly> GetThemeConfigurations()
	{
		if(!isConfigured())
			throw new FileNotFoundException("No configuration found for application.");
		var config = File.ReadAllLines(ConfigFilePath).ToList();
		var themeTimes = new Dictionary<string, TimeOnly>();
		foreach(var theme in config)
		{
			if(!theme.StartsWith("[["))
			{
				var split = theme.Split("=");
				var time = split[1];
				var themePath = split[0];
				themeTimes.Add(themePath, TimeOnly.Parse(time));
			}

		}
		return themeTimes;
	}

	private static bool isConfigured()
	{
		return File.Exists(ConfigFilePath);
	}
}