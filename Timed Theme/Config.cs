// See https://aka.ms/new-console-template for more information

using static Timed_Theme.Clock;

namespace Timed_Theme;

internal class Config
{
	public static readonly string ThemesPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Microsoft\\Windows\\Themes";
	public static readonly string ConfigFilePath = ThemesPath + "\\config.csc";
	public ThemeSchedule ThemeConfigurations { get; }

	public Config()
	{
		ThemeConfigurations = ThemeSchedule.fromFile(ConfigFilePath);
	}

	// todo improve prompts to be entered on the same line, make it a claasss?
	/// <summary>
	/// Runs the configuration prompts to setup times and themes for the app
	/// </summary>
	public void Configure()
	{
		if(!IsWin11())
			throw new PlatformNotSupportedException("This app is only supported on Windows 11 and 10.");
		if(true)
		{
			Console.WriteLine("How many themes would you like to set:");
			Console.WriteLine("1. Day and night (2 themes)");
			Console.WriteLine("2. Custom (n# of themes)");
			Console.WriteLine("3. Exit");
			Console.Write("Enter your choice: ");
			string? choice = Console.ReadLine();
			switch(choice)
			{
				case "1":
					ConfigThemes(2);
					break;
				case "2":
					Console.WriteLine("How many themes would you like to set? ");
					var input = Console.ReadLine();
					int n;
					while(!int.TryParse(input, out n))
					{
						//todo go back and add only not newlining
						Console.WriteLine("Invalid entry\n");
						Console.WriteLine("How many themes would you like to set?");
						Console.WriteLine("Please enter a number: ");
						input = Console.ReadLine();
					}
					ConfigThemes(n);
					break;
				case "3":
					throw new OperationCanceledException("User canceled configuration");
					break;
				case null:
					throw ErrorCodes.ErrorBadCommand();
					break;
				default:
					//todo: somesort of try again loop
					throw new NotImplementedException();
					break;
			}
			SaveThemeSettings();
			Exit(ErrorCodes.None);
		}
		else
		{
			Console.WriteLine("Event mode selected");
		}
	}

	/// <summary>
	/// Runs the configuration dialog for to setup the themes schedule
	/// </summary>
	/// <param name="themeQty">The number of themes to setup in the schedule</param>
	private void ConfigThemes(int themeQty)
	{
		//? dictionary of themes = <fullpath,filename>
		var themes = GetUsersThemes(true)
			.Zip(GetUsersThemes(false))
			.ToDictionary(tuple => tuple.First, tuple => tuple.Second);

		for(int i = 0; i < themeQty; i++)
		{
			var themeMode = themeQty == 2 ?
				//if there are 2 themes; select for 'day' then 'night'
				i == 0 ? "day theme" : "night theme"
				//else show which theme number they're now setting for
				: $"theme {i + 1}";
			var name = SetName();
			var time = SetTime(themeMode);
			Console.WriteLine("{1} time set to {0}", time, themeMode);
			Console.WriteLine("Select which theme to be used for {0}: ", themeMode);
			var themeName = GetThemeInput(themes.Values.ToList());
			var themePath = themes.First(x => x.Value == themeName).Key;
			ThemeConfigurations.Add(time, themePath, name, false);
		}
	}

	/// <summary>
	/// Runs dialog to set the name for a theme
	/// </summary>
	/// <returns>entered name, or null if no name is specified</returns>
	private string? SetName()
	{
		Console.WriteLine("Would you like to set a name for this theme (Y/n)? ");
		var continueNaming = Console.ReadLine();
		if(continueNaming == null || !continueNaming.Equals("n", StringComparison.OrdinalIgnoreCase) || !continueNaming.Equals("no", StringComparison.OrdinalIgnoreCase))
		{
			string? name = string.Empty;
			while(string.IsNullOrWhiteSpace(name))
			{
				Console.WriteLine("Theme name: ");
				name = Console.ReadLine();
				if(string.IsNullOrWhiteSpace(name))
					Console.WriteLine("Invalid name");
			}
			return name.Trim();
		}
		else
			return null;
	}

	/// <summary>
	/// Saves the theme with the LocalApplication data of the users windows themes
	/// </summary>
	/// <param name="nightTheme">The night theme.</param>
	/// <param name="nightTime">The time to switch to the night theme</param>
	/// <param name="dayTheme">The day theme.</param>
	/// <param name="dayTime">The day time switch to the day theme.</param>
	/// <returns>If the config file was successfully saved to the system</returns>
	private bool SaveThemeSettings()
	{
		try
		{
			var settings = File.CreateText(ConfigFilePath);
			settings.AutoFlush = true;
			var configSettings = ThemeConfigurations.Print(true);
			settings.Write(configSettings);
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
	/// Gets the theme selection from user.
	/// </summary>
	/// <param name="themes">The themes to offer.</param>
	/// <returns>The selected theme's index.</returns>
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
	private static TimeOnly SetTime(string themeMode)
	{
		Console.WriteLine($"Enter the time you want to change to {themeMode} (hh:mm:ss): ");
		var time = InputTime();
		Console.WriteLine($"Is {time.ToString("HH:mm")} ({time}) the correct time for {themeMode} to be activated? (y/N): ");
		var confirm = Console.ReadLine();
		//todo: add yes option in 'if'
		if(confirm != null && confirm.Equals("y", StringComparison.OrdinalIgnoreCase))
			return time;
		return SetTime(themeMode);
	}

	/// <summary>
	/// Console logic to change the displayed time.
	/// </summary>
	private static readonly EventHandler<ChangeEventArgs> ToggleTime = (_, e) =>
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
		//? print initial time to console
		ToggleTime(null, new(clock.Time, clock.Time));
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
	internal static List<string> GetUsersThemes(bool fullPath = true)
	{
		var dir = Directory.GetDirectories(ThemesPath);
		List<string> themes = new List<string>();
		Array.ForEach(dir, directory =>
		{
			var theme = new DirectoryInfo(directory).GetFiles("*.theme", SearchOption.TopDirectoryOnly);
			themes.Add(fullPath ? theme[0].FullName : theme[0].Name);
		});
		foreach(string file in Directory.GetFiles(ThemesPath, "*.theme"))
			if(fullPath)
				themes.Add(file);
			else
			{
				var path = file.Split("\\", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				themes.Add(path.Length > 0 ? path[^1] : file);
			}
		return themes;
	}

	private static bool isConfigured()
	{
		return File.Exists(ConfigFilePath);
	}
}