// See https://aka.ms/new-console-template for more information
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using System.Text;
using Timed_Theme;

Console.WriteLine("Hello, World!");

// themes loc: C:\Users\{user}\AppData\Roaming\Microsoft\Windows\Themes

DateTime nightMode = new DateTime();
DateTime dayMode = nightMode;

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

if(!isWin11())
	//	ERROR_NOT_SUPPORTED
	//The request is not supported.
	Environment.Exit(0x32); //50

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

var themes = getThemes();

if(getMode() == 1)
{
	Console.WriteLine("Time mode selected");
	var time = setTime();
	Console.WriteLine("Night mode set to {0}", time);

	Console.WriteLine("Select which theme to be used for night (mode): ");
	switchTheme(themes);
}
else
{
	Console.WriteLine("Event mode selected");
}


#region Functions

List<String> getThemes()
{
	string user = Environment.UserName;
	string path = $"C:\\Users\\{user}\\AppData\\Local\\Microsoft\\Windows\\Themes";
	var files = Directory.GetFiles(path);
	Console.WriteLine(string.Join("\n", files));
	return files.ToList();
}

void switchTheme(List<string> themes)
{
	int i = 0;
	themes.ForEach(theme => Console.WriteLine($"({i++}). {theme}"));
}

string GetThemeName()
{
	StringBuilder themeNameBuffer = new StringBuilder(260);
	var error = GetCurrentThemeName(themeNameBuffer, themeNameBuffer.Capacity, null, 0, null, 0);
	if(error != 0)
		Marshal.ThrowExceptionForHR(error);
	return themeNameBuffer.ToString();
}

DateTime setTime()
{
	DateTime night = DateTime.MinValue;
	Console.WriteLine("Enter the time you want to change to night mode (hh:mm:ss): ");
	toggleTime();
	string? time = Console.ReadLine();
	if(time == null)
	{
		Console.WriteLine("Invalid time, try again(y/N): ");
		string? retry = Console.ReadLine();
		if(retry == null)
			Environment.Exit(0x16); //22
		else if(retry.Equals("y", StringComparison.OrdinalIgnoreCase))
			setTime();
		else
		{
			Console.WriteLine("Exiting...");
			Environment.Exit(0); //22
		}
	}
	else
	{
		try
		{
			night = DateTime.Parse(time);
		}
		catch(FormatException e)
		{
			Console.WriteLine("Invalid format, please try again.");
			Console.WriteLine(e.Message);
			night = setTime();
		}
	}
	Console.WriteLine($"Is {night} the correct time for dark mode to be activated? (y/N): ");
	var correct = Console.ReadLine();
	if(correct != null && correct.Equals("y", StringComparison.OrdinalIgnoreCase))
		return night;
	else
		return setTime();
}

void toggleTime()
{
	Console.BackgroundColor = ConsoleColor.Green;
	Console.ForegroundColor = ConsoleColor.White;

	TimeOnly time = TimeOnly.MinValue;
	Console.WriteLine(time.ToString("HH:mm"));
	

	var key = Console.ReadKey();
	while(key.Key != ConsoleKey.Enter)
	{
		try
		{
			key = Console.ReadKey(true);
			switch(key.Key)
			{
				case ConsoleKey.UpArrow:
					time = time.AddMinutes(1);
					break;
				default:
					break;
			}

		}
		catch(Exception e)
		{
			Console.Error.WriteLine(e.Message);
			throw e;
		}

	}
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