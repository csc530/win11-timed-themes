// See https://aka.ms/new-console-template for more information
using System.Runtime.InteropServices;
using System.Text;
using static Timed_Theme.Clock;

namespace Timed_Theme
{
	internal class Program
	{
		readonly static string user = Environment.UserName;
		static readonly string themesPath = $"C:\\Users\\{user}\\AppData\\Local\\Microsoft\\Windows\\Themes";

		private static void Main(string[] args)
		{
			#region Functions
			int exit(ErrorCodes err)
			{
				int code = (int)err;
				Environment.Exit(code);
				return code;
			}

			int getMode()
			{
				Console.WriteLine("Would you like to change theme by time or event?");
				Console.WriteLine("1. Time");
				Console.WriteLine("2. Event");
				Console.Write("Enter your choice: ");
				string? choice = Console.ReadLine();
				if(choice == null)
					return exit(ErrorCodes.ERROR_BAD_COMMAND);
				else if(choice == "1" || choice.Equals("time", StringComparison.OrdinalIgnoreCase) || choice.Equals("t", StringComparison.OrdinalIgnoreCase))
					return 1;
				else if(choice == "2" || choice.Equals("event", StringComparison.OrdinalIgnoreCase) || choice.Equals("e", StringComparison.OrdinalIgnoreCase))
					return 2;
				else
				{
					Console.WriteLine("Invalid choice, try again");
					// * pretty comfortable recursing as it will be a stuupid # of times before a stackoverflows occurs
					return getMode();
				}
			}

			bool isWin11()
			{
				OperatingSystem os = Environment.OSVersion;
				PlatformID pid = os.Platform;
				int ver = os.Version.Major;
				//print all vars in one line
				Console.WriteLine("OS: {0}\nPlatform: {1}\nVersion: {2}", os, pid, ver);
				//check for Windows 10+
				if(pid == PlatformID.Win32NT && ver >= 10)
				{
					Console.WriteLine("Windows 10 or higher detected");
					return true;
				}
				else
				{
					Console.WriteLine("Windows 10 or higher not detected");
					Console.WriteLine("This program only works on Windows 10 or higher");
					return false;
				}
			}


			List<string> getThemes()
			{
				var files = Directory.GetFiles(themesPath);
				var dir = Directory.GetDirectories(themesPath);
				var list = files.ToList();
				list.AddRange(dir);
				List<string> themes = new List<string>();
				list.ForEach(path =>
				{
					var pathList = path.Split("\\", StringSplitOptions.RemoveEmptyEntries);
					var name = pathList[^1].Split(".", 1)[0];
					themes.Add(name);
				});
				return themes;
			}

			string getThemeInput(List<string> themes)
			{
				int i = 1;
				themes.ForEach(theme => Console.WriteLine($"({i++}). {theme}"));
				Console.WriteLine();
				Console.Write("Select a theme by entering its numbered position: ");
				string? select = Console.ReadLine();
				bool isNum = int.TryParse(select, out int index);
				if(isNum && themes.Count > --index)
				{
					Console.Write($"Confirm {themes[index]} as night theme? (y/N): ");
					string? correct = Console.ReadLine();
					Console.WriteLine();
					if(correct != null && correct.Equals("y", StringComparison.OrdinalIgnoreCase))
						return themes[index];
					else
						return getThemeInput(themes);
				}
				else
				{
					Console.WriteLine("Invalid number, out of range input number between 1 - {0}", themes.Count);
					Console.WriteLine();
					return getThemeInput(themes);
				}
			}

			string GetThemeName()
			{
				StringBuilder themeNameBuffer = new StringBuilder(260);
				var error = GetCurrentThemeName(themeNameBuffer, themeNameBuffer.Capacity, null, 0, null, 0);
				if(error != 0)
					Marshal.ThrowExceptionForHR(error);
				return themeNameBuffer.ToString();
			}

			TimeOnly setTime()
			{
				Console.WriteLine("Enter the time you want to change to night mode (hh:mm:ss): ");
				TimeOnly night = inputTime();
				Console.WriteLine($"Is {night.ToString("HH:mm")} ({night}) the correct time for dark mode to be activated? (y/N): ");
				string? correct = Console.ReadLine();
				if(correct != null && correct.Equals("y", StringComparison.OrdinalIgnoreCase))
					return night;
				else
					return setTime();
			}

			void toggleTime(object? sender, ChangeEventArgs e)
			{
				string time = e.newTime.ToString("HH:mm");
				Console.CursorLeft = Console.WindowWidth / 2 - time.Length;
				Console.Write(time);
				Console.CursorLeft -= time.Length - 2;
			}

			TimeOnly inputTime()
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Clock clock = new Clock();
				EventHandler<ChangeEventArgs> changeConsoleTime = toggleTime;
				clock.OnChangeHandler += changeConsoleTime;

				ConsoleKeyInfo key = new ConsoleKeyInfo();
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
			#endregion
			
			// todo improve prompts to be entered on the same line, make it a claasss?

			if(!isWin11())
				exit(ErrorCodes.ERROR_NOT_SUPPORTED); //50


			List<string> themes = getThemes();

			if(getMode() == 1)
			{
				Console.WriteLine("Time mode selected");
				TimeOnly nightTime = setTime();
				Console.WriteLine("Night time set to {0}", nightTime);
				Console.WriteLine();
				
				TimeOnly dayTime = setTime();
				Console.WriteLine("Day time set to {0}", dayTime);
				Console.WriteLine();
				
				
				Console.WriteLine("Select which theme to be used for night (mode): ");
				var nightTheme = getThemeInput(themes);
				Console.WriteLine();
				Console.WriteLine("Select which theme to be used for day (mode): ");
				
				
				var dayTheme = getThemeInput(themes);
				saveTheme(nightTheme,nightTime, dayTheme, dayTime);
				Console.WriteLine("Theme settings saved.");
				Console.WriteLine();
				exit(ErrorCodes.SUCCESS);
			}
			else
			{
				Console.WriteLine("Event mode selected");
			}

			bool saveTheme(string nightTheme,TimeOnly nightTime, string dayTheme, TimeOnly dayTime)
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


			#region get themes loc stuff

			[DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
			static extern int GetCurrentThemeName(StringBuilder pszThemeFileName,
			int dwMaxNameChars,
			StringBuilder? pszColorBuff,
			int dwMaxColorChars,
			StringBuilder? pszSizeBuff,
			int cchMaxSizeChars);
			Console.WriteLine(GetThemeName());

			[DllImport("shell32.dll")]
			static extern int SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);
		}
	}
}
#endregion