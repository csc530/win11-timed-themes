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

    public override string ToString() => $"{Name}: ({Path})";

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Path);
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
        else if (arrayIndex < 0)
            throw new ArgumentException("The array index cannot be negative.");
        else if (arrayIndex >= array.Length) throw new ArgumentException("The array index is too large.");
        else
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
        if (!obj.PerfectParse(text))
            return obj;
        //throw new ArgumentException("The string is not in the correct format."); todo
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
        if (Count == 0)
            return;
        //Big blessups to Abdullah Nabil
        //https://stackoverflow.com/questions/71883411/changing-windows-theme-in-c-sharp
        var process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments =
            $"/c '\u0022'{GetThemeFor(TimeOnly.FromDateTime(DateTime.Now))!.Value.Path}'\u0022'";
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.Start();
        process.WaitForExit();
    }


    /// <summary>
    /// Gets the theme associated with the given time.
    /// </summary>
    /// <param name="time">The time</param>
    /// <returns>The selected theme for the given time</returns>
    public Theme? GetThemeFor(TimeOnly time)
    {
        switch (Count)
        {
            case 0:
                return null;
            case 1:
                return Themes.Values.First();
            default:
            {
                var keys = Themes.Keys.ToList();
                var values = Themes.Values.ToList();
                var index = keys.BinarySearch(time);
                if (index >= 0) return values[index];
                index = ~index;
                //if the time is before the first time in the schedule, return the last theme
                //? ie now=2pm and schedule is 3pm-4pm; then the 4pm time is returned
                if (index == 0)
                    return values.Last();
                //if the time is after the last time in the schedule, return the first theme
                //? ie now=5pm and schedule is 3pm-4pm; then the 3pm time is returned
                else if (index == keys.Count)
                    return values.First();
                //if the time is between two times in the schedule, return the theme associated with the earlier time
                //? ie now=3:30pm and schedule is 3pm-4pm; then the 3pm time is returned
                else
                    return values[index - 1];
            }
        }
    }

    public KeyValuePair<TimeOnly, Theme> NextTheme()
    {
        if (Count == 0)
            return Themes.First();
        var keys = Themes.Keys.ToList();
        var index = keys.BinarySearch(CurrentTime);
        //mod by the length so that if it's the last theme, it loops back to the first
        if (index >= 0) return Themes.ElementAt((index + 1) % keys.Count);
        index = ~index;
        if(index == keys.Count)
            return Themes.First();
        //no need to add 1 because the index is already the next theme (BinarySearch doc)
        return Themes.ElementAt(~index % keys.Count);
    }
    
    public KeyValuePair<TimeOnly,Theme> PreviousTheme()
    {
        if (Count == 0)
            return Themes.First();
        var keys = Themes.Keys.ToList();
        var index = keys.BinarySearch(CurrentTime);
        //what happens here is
        //we subtract ine from the index to get the previous theme
        //then we add the length of the list to it to make sure it's positive
        //then we mod it by the length of the list to make sure it's in the range of the list
        //this is done so that if it's the first theme, it loops back to the last; i.e. the previous theme
        //? tl;dr the keys count cancel out and when it's zero it loops back to the last
        if (index >= 0) return Themes.ElementAt((index - 1 + keys.Count) % keys.Count);
        index = ~index;
        if(index == 0)
            return Themes.Last();
        return Themes.ElementAt((index - 1 + keys.Count) % keys.Count);
    }

    public static TimeOnly CurrentTime => TimeOnly.FromDateTime(DateTime.Now);
}