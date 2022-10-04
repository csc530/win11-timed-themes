// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

DateTime nightMode = new DateTime();
var dayMode = nightMode;

Console.WriteLine(nightMode);
Console.WriteLine(dayMode);

Console.WriteLine(dayMode);
Console.WriteLine(nightMode);


bool checkEnv()
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
		return false;
	}
}

checkEnv();
return -1;
