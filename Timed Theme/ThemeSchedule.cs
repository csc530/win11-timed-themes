using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Timed_Theme;

/// <summary>
///     A simple container for a theme's name and path.
/// </summary>
internal readonly struct Theme : IEquatable<Theme>
{
    public Theme(string name, string path)
    {
        Name = name;
        Path = path;
    }

    public string Name { get; init; }
    public string Path { get; init; }

    public override bool Equals(object? obj)
    {
        return obj is Theme other && Equals(other);
    }

    public bool Equals(Theme other)
    {
        return Name == other.Name && Path == other.Path;
    }

    public static bool operator ==(Theme left, Theme right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Theme left, Theme right)
    {
        return !left.Equals(right);
    }
}

/// <summary>
///     The theme schedule.
/// </summary>
internal class ThemeSchedule : IDictionary<TimeOnly, Theme>
{
    private static readonly TimeSpan DaySpan = TimeSpan.FromDays(1);
    public readonly Dictionary<TimeOnly, Theme> Themes;

    public ThemeSchedule(ICollection<TimeOnly> keys, ICollection<Theme> values)
    {
        Themes = new Dictionary<TimeOnly, Theme>();
    }

    public ThemeSchedule()
    {
        Themes = new Dictionary<TimeOnly, Theme>();
    }

    public ICollection<TimeOnly> Keys => Themes.Keys;
    public ICollection<Theme> Values => Themes.Values;
    public int Count => Themes.Count;
    public bool IsReadOnly { get; }


    public Theme this[TimeOnly key]
    {
        get => Themes[key];
        set => Themes[key] = value;
    }

    public void Add(TimeOnly key, Theme value)
    {
        Themes.Add(key, value);
    }


    public bool ContainsKey(TimeOnly key)
    {
        return Themes.ContainsKey(key);
    }


    bool IDictionary<TimeOnly, Theme>.Remove(TimeOnly key)
    {
        return Themes.Remove(key);
    }


    public bool TryGetValue(TimeOnly key, out Theme value)
    {
        return Themes.TryGetValue(key, out value);
    }


    public IEnumerator<KeyValuePair<TimeOnly, Theme>> GetEnumerator()
    {
        return Themes.GetEnumerator();
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    public void Add(KeyValuePair<TimeOnly, Theme> item)
    {
        Themes.Add(item.Key, item.Value);
    }


    public void Clear()
    {
        Themes.Clear();
    }


    public bool Contains(KeyValuePair<TimeOnly, Theme> item)
    {
        return Themes.Contains(item);
    }


    public void CopyTo(KeyValuePair<TimeOnly, Theme>[] array, int arrayIndex)
    {
        if (array.Length - arrayIndex < Themes.Count)
            throw new ArgumentException("The array is too small to copy the themes to.");
        if (arrayIndex < 0)
            throw new ArgumentException("The array index cannot be negative.");
        if (arrayIndex >= array.Length) throw new ArgumentException("The array index is too large.");
        for (var i = arrayIndex; i < array.Length; i++)
            array[i] = new KeyValuePair<TimeOnly, Theme>(Keys.ElementAt(i), Values.ElementAt(i));
    }

    public bool Remove(KeyValuePair<TimeOnly, Theme> item)
    {
        if (Themes.ContainsKey(item.Key) && Themes[item.Key] == item.Value)
            return Themes.Remove(item.Key);
        return false;
    }

    /// <summary>
    ///     Adds a theme or list of themes to the schedule.
    /// </summary>
    /// <param name="text">A string containing a single theme or a list of themes. In the same format as ThemeSchedule Print().</param>
    /// <returns>True if the theme(s) were successfully added, false if there was an error.</returns>
    public static ThemeSchedule Parse(string text)
    {
        var obj = new ThemeSchedule();
        obj.PerfectParse(text);
        return obj;
    }

    private bool PerfectParse(string text)
    {
        var lines = text.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var values = new List<KeyValuePair<TimeOnly, Theme>>();
        string? name = null;
        foreach (var line in lines)
        {
            var parts = line.Split('@', StringSplitOptions.TrimEntries);
            if (parts.Length == 1 && line.StartsWith("[["))
                name = line.Substring(2, line.Length - 4);
            else if (parts.Length != 2)
                return false;
            else
                values.Add(KeyValuePair.Create(TimeOnly.Parse(parts[1]),
                    new Theme(name ?? string.Empty, parts[0])));
        }

        //todo add option for overwrite or no
        foreach (var pair in values)
            Add(pair);
        return true;
    }


    /// <summary>
    ///     Gets the themes' path in the schedule.
    /// </summary>
    /// <returns>A list of string.</returns>
    public List<string> GetThemePaths()
    {
        return Themes.Values.Select(x => x.Path).ToList();
    }


    /// <summary>
    ///     Gets the themes' names in the schedule.
    /// </summary>
    /// <returns>A list of theme names.</returns>
    public List<string> GetThemeNames()
    {
        return Themes.Values.Select(x => x.Name).ToList();
    }


    public string Print(bool withNames = false)
    {
        var sb = new StringBuilder();
        foreach (var theme in Themes)
        {
            if (withNames)
                sb.AppendLine($"[[{theme.Value.Name}]]");
            sb.Append(theme.Value.Path);
            sb.Append(" @ ");
            sb.AppendLine(theme.Key.ToString());
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Sets the current desktop theme to the current time's associated theme in the schedule.
    /// </summary>
    public void SetCurrentTheme()
    {
        Console.WriteLine("SetCurrentTheme");
        if (Count == 0)
            return;
        //Big blessups to Abdullah Nabil
        //https://stackoverflow.com/questions/71883411/changing-windows-theme-in-c-sharp
        var tokyo1 = "call " + '\u0022' + GetCurrentTheme()!.Value.Path + '\u0022';
        var process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = "/c " + tokyo1;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = false;
        process.StartInfo.Verb = "runas";
        process.Start();
        process.Dispose();
    }

    /// <summary>
    ///     Gets the theme associated with the current time.
    /// </summary>
    /// <returns>The Theme for the current time</returns>
    public Theme? GetCurrentTheme()
    {
        switch (Count)
        {
            case 0:
                return null;
            case 1:
                return Themes.Values.First();
            default:
            {
                var now = TimeOnly.FromDateTime(DateTime.Now);
                var keys = Themes.Keys.ToList();
                var values = Themes.Values.ToList();
                var index = keys.BinarySearch(now);
                if (index < 0)
                {
                    index = ~index;
                    //if the time is before the first time in the schedule, return the last theme
                    //? ie now=2pm and schedule is 3pm-4pm; then the 4pm time is returned
                    if (index == 0)
                        return values.Last();
                    //if the time is after the last time in the schedule, return the first theme
                    //? ie now=5pm and schedule is 3pm-4pm; then the 3pm time is returned
                    if (index == keys.Count)
                        return values.First();
                    //if the time is between two times in the schedule, return the theme associated with the earlier time
                    //? ie now=3:30pm and schedule is 3pm-4pm; then the 3pm time is returned
                    return values[index - 1];
                }
                return values[index];
            }
        }
    }
}