using System.Reflection.Metadata;
using System.Text;

namespace Timed_Theme
{
	/// <summary>
	/// The theme schedule.
	/// </summary>
	internal class ThemeSchedule
	{
		private readonly TimeSpan _daySpan = TimeSpan.FromDays(1);
		readonly private Dictionary<TimeOnly, Theme> _themes;
		/// <summary>
		/// A simple container for a theme's name and path.
		/// </summary>
		readonly internal struct Theme
		{
			public Theme(string name, string path)
			{
				Name = name;
				Path = path;
			}

			public string Name { get; init; }
			public string Path { get; init; }
		}
		public ThemeSchedule()
		{
			_themes = new Dictionary<TimeOnly, Theme>();
		}

		/// <summary>
		/// Adds a theme to the schedule.
		/// </summary>
		/// <param name="theme">A pair of the time and path to the theme</param>
		/// <param name="name">An optional name for the theme</param>
		/// <param name="overwrite">If true replaces any existing theme in the given time.
		/// If false the new theme will not be added if one exists in the given
		/// </param>
		/// <returns>True if the theme was successfully added, false if it would overwrite an extsting time's theme</returns>
		public bool Add(KeyValuePair<TimeOnly, string> theme, string? name = null, bool overwrite = false)
		{
			return Add(theme.Key, theme.Value, name, overwrite);
		}

		/// <summary>
		/// Adds multiple themes to the schedule.
		/// </summary>
		/// <param name="themes">An IEnumerable list of theme time and path KeyValuePairs</param>
		/// <param name="overwrite">If true overwrites previous theme for a given time. Default is false;
		/// leaves the schedule unchanged if adding to a set time.</param>
		/// <returns>True if the entire list was successfully added, false if there were one or more existing entries for a time</returns>
		public bool Add(IEnumerable<KeyValuePair<TimeOnly, string>> themes, bool overwrite = false)
		{
			if(overwrite)
			{
				foreach(KeyValuePair<TimeOnly, string> pair in themes)
					Add(pair, overwrite: overwrite);
				return true;
			}
			else
			{
				//see if there are a common key between the two dictionaries
				IEnumerable<KeyValuePair<TimeOnly, string>> themePairs = themes as KeyValuePair<TimeOnly, string>[] ?? themes.ToArray();
				bool duplicateEntry = themePairs
					.Where(theme => _themes.ContainsKey(theme.Key))
					.ToArray()
					.Length > 0;

				if(duplicateEntry)
					return false;
				//if there are no common keys, add all the themes
				else
				{
					foreach(KeyValuePair<TimeOnly, string> pair in themePairs)
						Add(pair, overwrite: overwrite);
					return true;
				}
			}
		}

		/// <summary>
		/// Adds a theme to the schedule.
		/// </summary>
		/// <param name="time">The time to set the theme.</param>
		/// <param name="path">Full file path to the theme file.</param>
		/// <param name="name">Optional name for the theme</param>
		/// <param name="overwrite">If true overwrites previous theme for a given time. Default is false;
		/// leaves the schedule unchanged if adding to a set time.</param>
		/// <returns>True if the theme was successfully added, false if it would overwrite an extsting time's theme</returns>
		public bool Add(TimeOnly time, string path, string? name = null, bool overwrite = false)
		{
			name ??= "theme " + _themes.Count + 1 + "(" + path.Split('\\', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last() + ")";
			if(!overwrite && _themes.ContainsKey(time))
				return false;
			else
			{
				_themes.Add(time, new Theme(name, path));
				return true;
			}
		}


		public void Remove(string name, RemoveMode mode = RemoveMode.RemoveFirst)
		{
			switch(mode)
			{
				case RemoveMode.RemoveFirst:
				{
					foreach(var theme in _themes)
						if(theme.Value.Name == name)
						{
							_themes.Remove(theme.Key);
							return;
						}
					break;
				}
				case RemoveMode.RemoveLast:
				{
					for(int i = _themes.Count - 1; i >= 0; i--)
					{
						var theme = _themes.ElementAt(i);
						if(theme.Value.Name == name)
						{
							_themes.Remove(theme.Key);
							return;
						}
					}
					break;
				}
				default:
				case RemoveMode.RemoveAll:
				{
					for(int i = _themes.Count - 1; i >= 0; i--)
					{
						var theme = _themes.ElementAt(i);
						if(theme.Value.Name == name)
							_themes.Remove(theme.Key);
					}
					var toRemove = _themes
						.Where(x => x.Value.Name == name)
						.Select(x => x.Key)
						.ToList();
					foreach(var time in toRemove)
						_themes.Remove(time);
					break;
				}
			}
		}

		public void ClearSchedule()
		{
			_themes.Clear();
		}

		public void Remove(TimeOnly time)
		{
			_themes.Remove(time);
		}

		public void RemoveAt(int index)
		{
			//? get list of themes from this object (sorted by time)
			//find the ith theme by time in the list
			var theme = _themes.ToList()[index].Key;
			//remove it from the dictionary
			Remove(theme);
		}

		/// <summary>
		/// Gets the themes' path in the schedule.
		/// </summary>
		/// <returns>A list of string.</returns>
		public List<string> GetThemePaths()
		{
			return _themes.Values.Select(x => x.Path).ToList();
		}

		/// <summary>
		/// Gets the themes' names in the schedule.
		/// </summary>
		/// <returns>A list of theme names.</returns>
		public List<string> GetThemeNames()
		{
			return _themes.Values.Select(x => x.Name).ToList();
		}

		/// <summary>
		/// Gets the themes in the schedule.
		/// </summary>
		/// <returns>A Dictionary of times and its associated theme.</returns>
		public Dictionary<TimeOnly, Theme> GetThemes()
		{
			return new Dictionary<TimeOnly,Theme>(_themes);
		}

		/// <summary>
		/// The removal mode.
		/// </summary>
		internal enum RemoveMode
		{
			RemoveAll,
			RemoveFirst,
			RemoveLast
		}

		public string Print(bool withNames = false)
		{
			var sb = new StringBuilder();
			foreach(var theme in _themes)
			{
				if(withNames)
					sb.AppendLine($"[[{theme.Value.Name}]]");
				sb.Append(theme.Value.Path);
				sb.Append(" @ ");
				sb.AppendLine(theme.Key.ToString());
			}
			return sb.ToString();
		}

	}
}
