// See https://aka.ms/new-console-template for more information
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Timed_Theme;
using static Timed_Theme.Clock;
// todo improve prompts to be entered on the same line, make it a claasss?

DateTime nightMode = new DateTime();
DateTime dayMode = nightMode;

if(!isWin11())
	Environment.Exit(ErrorCodes.ERROR_NOT_SUPPORTED); //50



var themes = getThemes();

if(getMode() == 1)
{
	Console.WriteLine("Time mode selected");
	TimeOnly time = setTime();
	Console.WriteLine("Night mode set to {0}", time);
	Console.WriteLine();
	Console.WriteLine("Select which theme to be used for night (mode): ");
	setTheme(themes);
}
else
{
	Console.WriteLine("Event mode selected");
}


#region Functions
int getMode()
{
	Console.WriteLine("Would you like to change theme by time or event?");
	Console.WriteLine("1. Time");
	Console.WriteLine("2. Event");
	Console.Write("Enter your choice: ");
	string? choice = Console.ReadLine();
	if(choice == null)
	{ //	ERROR_BAD_COMMAND
	  //The device does not recognize the command.
		Environment.Exit(0x16); //22
		return -1;
	}
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


List<String> getThemes()
{
	string user = Environment.UserName;
	string path = $"C:\\Users\\{user}\\AppData\\Local\\Microsoft\\Windows\\Themes";
	var files = Directory.GetFiles(path);
	var dir = Directory.GetDirectories(path);
	var list = files.ToList();
	list.AddRange(dir);
	List<String> themes = new List<string>();
	list.ForEach(path =>
	{
		var pathList = path.Split("\\", StringSplitOptions.RemoveEmptyEntries);
		var name = pathList[^1].Split(".", 1)[0];
		themes.Add(name);
	});
	return themes;
}

string setTheme(List<string> themes)
{
	var theme = getThemeInput(themes);
	return Path.GetRandomFileName();
}

string getThemeInput(List<string> themes)
{
	int i = 1;
	themes.ForEach(theme => Console.WriteLine($"({i++}). {theme}"));
	Console.WriteLine();
	Console.Write("Select a theme by entering its numbered position: ");
	var select = Console.ReadLine();
	int index;
	var isNum = int.TryParse(select, out index);
	if(isNum && themes.Count > --index)
	{
		Console.Write($"Confirm {themes[index]} as night theme? (y/N): ");
		var correct = Console.ReadLine();
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

/// Schedule the given action for the given time.
/// courtesy of Steve Cherry @https://stackoverflow.com/questions/1493203/alarm-clock-application-in-net
async void ScheduleAction(Action action, DateTime ExecutionTime)
{
	try
	{
		CancellationToken token = new CancellationToken();
		await Task.Delay(((int)ExecutionTime.Subtract(DateTime.Now).TotalMilliseconds), token);
		action();
	}
	catch(Exception)
	{
		// Something went wrong
	}
}
#endregion

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
static extern Int32 SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);
#endregion