namespace Timed_Theme
{
	/// <summary>
	/// A clock to store the time of day 00:00:00 to 23:59:59
	/// As well as to tick the clock forward and set alarms of events for certain times
	/// </summary>
	internal class Clock
	{
		private TimeOnly _time = TimeOnly.MinValue;
		private Timer _timer;

		/// <summary>
		/// An event to trigger once every time the Time change's to it's indexed time
		/// </summary>
		public readonly Dictionary<TimeOnly, EventHandler<TimeOnly>> Alarms;
		/// <summary>
		/// Handler when the time changes as a result of calling <see cref="Start"/> triggering tick events
		/// </summary>
		public event EventHandler<TickEventArgs>? OnTickHandler;
		private TimeSpan _tickIntervalSpan = new();
		/// <summary>
		/// Method called called when the clock ticks. This method is called every time the clock ticks.
		/// <note type="inherit">The <see cref="OnTickHandler"/> event is called after this method and <see cref="Time"/> is increased</note>
		/// </summary>
		/// <param name="state">The clock's current state</param>
		protected virtual void Tick(object? state)
		{
			Increment(_tickIntervalSpan);
			OnTickHandler?.Invoke(this, new TickEventArgs(TickInterval, Time));
		}
		/// <summary>
		/// The interval between each tick in seconds
		/// </summary>
		public double TickInterval
		{
			get => _tickIntervalSpan.TotalSeconds;
			set => _tickIntervalSpan = TimeSpan.FromSeconds(value);
		}
		/// <summary>
		/// The event arguments for the <see cref="OnTickHandler"/> event
		/// </summary>
		public class TickEventArgs : EventArgs
		{
			readonly public double elapsedTime;
			readonly public TimeOnly time;
			/// <summary>
			/// The time that the clock was at when the tick event was triggered and set interval for the tick
			/// </summary>
			/// <param name="interval">The elapsed time in seconds from the </param>
			/// <param name="time">The current time; after the tick</param>
			public TickEventArgs(double interval, TimeOnly time)
			{
				this.elapsedTime = interval;
				this.time = time;
			}
		}
		/// <summary>
		/// Handler for each change in the clock's time
		/// Triggered when clock tick's and when the time is set
		/// </summary>
		public EventHandler<ChangeEventArgs>? OnChangeHandler { get; set; }
		/// <summary>
		/// Method called when the <see cref="Clock">Clock's</see> time value changes.
		/// </summary>
		/// <remarks><note type="important">This is called when the clock is <seealso cref="Start">started</seealso> and ticking aswell but also in direct reassingments of the <see cref="Time">Time</see> value.</note></remarks>
		/// <param name="old">The previous <see cref="TimeOnly">Time</see> value the clock contained</param>
		protected virtual void OnChange(TimeOnly old) => OnChangeHandler?.Invoke(this, new ChangeEventArgs(old, Time));
		/// <summary>
		/// The <see cref="TimeOnly">Time</see> value the clock contains
		/// </summary>
		public TimeOnly Time
		{
			get => _time;
			set
			{
				TimeOnly prev = _time;
				_time = value;
				OnChange(prev);
				if(Alarms.TryGetValue(_time, out EventHandler<TimeOnly>? alarm))
					alarm.Invoke(this, _time);
			}
		}

		#region Constructors

		/// <summary>
		/// Creates a new <see cref="Clock">Clock</see> instance with the default <see cref="TimeOnly">Time</see> value of <see cref="TimeOnly.MinValue">TimeOnly.MinValue</see>
		/// </summary>
		/// <inheritdoc/>
		/// <overloads>Creates a new <see cref="Clock">Clock</see> instance</overloads>
		public Clock()
		{
			_timer = new(Tick, null, Timeout.Infinite, Timeout.Infinite);
			Alarms = new();
			TickInterval = 1;
		}

		/// <summary>
		/// Creates a new <see cref="Clock">Clock</see> with the given parsed <see cref="Time">Time</see> value
		/// </summary>
		/// <param name="time">A string representing a time of day</param>
		public Clock(String time) : this()
		{
			Time = TimeOnly.Parse(time);
		}

		/// <summary>
		/// Creates a new <see cref="Clock">Clock</see> instance with the specified <see cref="TimeOnly">Time</see> value
		/// </summary>
		/// <param name="hour">The hour 0 through 23</param>
		/// <param name="minute">The minute 0 through 59</param>
		/// <param name="second">The second 0 through 59</param>
		public Clock(int hour, int minute, int second) : this()
		{
			Time = new(hour, minute, second);
		}

		/// <summary>
		/// Creates a new <see cref="Clock">Clock</see> instance with the specified <see cref="TimeOnly">Time</see> value at the beggining ogf the miunte
		/// at zero seconds
		/// </summary>
		/// <param name="hour">The hour 0 through 23</param>
		/// <param name="minute">The minute 0 through 59</param>
		public Clock(int hour, int minute) : this(hour, minute, 0) { }

		/// <summary>
		/// Creates a new <see cref="Clock">Clock</see> instance with the specified <see cref="TimeOnly">Time</see> value at the beggining of the hour
		/// zero seconds and the 0 minute
		/// </summary>
		/// <param name="hour">The hour 0 through 23</param>
		public Clock(int hour) : this(hour, 0, 0) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Clock"/> class.
		/// With the given <see cref="TimeOnly"/> value using the specified number of ticks.
		/// </summary>
		/// <param name="ticks">A positive number of ticks that represents the time of day.</param>
		public Clock(long ticks) : this()
		{
			Time = new TimeOnly(ticks);
		}

		#endregion

		#region inc & dec
		/// <summary>
		/// Increase the current Time by the given hours, minutes, and seconds
		/// </summary>
		/// <param name="hours">Number of hours to add</param>
		/// <param name="minutes">Number of minutes to add</param>
		/// <param name="seconds">Number of seconds to add</param>
		/// <returns>the number of excess days if any that resulted from wrapping during this addition operation</returns>
		public int Increment(int hours, int minutes, int seconds) => Increment(new(hours, minutes, seconds));

		/// <summary>
		/// Increases the current time by adding the specified <see cref="TimeSpan"/> to it. Returns the number of wrapped days, if any, from the sum of the time and specifed span
		/// </summary>
		/// <param name="span">Positive or negative time interval</param>
		/// <returns>the number of excess days if any that resulted from wrapping during this addition operation</returns>
		public int Increment(TimeSpan span)
		{
			Time = Time.Add(span, out var wrappedDays);
			return wrappedDays;
		}

		/// <summary>
		/// Decreases the current time by subtracting the given hours, minutes, and seconds from it
		/// </summary>
		/// <param name="hours">positive number of hours</param>
		/// <param name="minutes">positive number of minutes</param>
		/// <param name="seconds">positive number of seconds</param>
		/// <returns>the number of excess (loss) days if any that resulted from wrapping during this subtraction operation</returns>
		public int Decrement(int hours, int minutes, int seconds) => Decrement(new(hours, minutes, seconds));

		/// <summary>
		/// Decreases the current time by subtracting the specified TimeSpan and returns the number of excess days, if any
		/// </summary>
		/// <param name="span">time interval</param>
		/// <returns>the number of excess (loss) days if any that resulted from wrapping during this addition operation</returns>
		public int Decrement(TimeSpan span)
		{
			Time = Time.Add(-span, out var wrappedDays);
			return wrappedDays;
		}
		#endregion

		#region toString
		/// <summary>
		/// Converts the current Clock instance to its equivalent short time string representation using the formatting conventions of the current culture.
		/// </summary>
		/// <returns>The short time string representation of the time in the current culture</returns>
		public override string ToString() => Time.ToString();

		/// <summary>
		/// <see cref="TimeOnly.ToString(string?)"/>
		/// </summary>
		/// <param name="format"></param>
		/// <returns>The formatted time string</returns>
		public string ToString(string format)
		{
			return Time.ToString(format);
		}

		/// <summary>
		/// <see cref="TimeOnly.ToLongTimeString"/>
		/// </summary>
		/// <returns></returns>
		public string ToLongString() => Time.ToLongTimeString();
		#endregion

		/// <summary>
		/// Sync's the clock's time with the system's time
		/// </summary>
		public void SyncTime() => Time = TimeOnly.FromDateTime(DateTime.Now);

		/// <summary>
		/// Starts the clock
		/// Ticking at each given <see cref="TickInterval">TickInterval</see>
		/// </summary>
		public void Start()
		{
			SyncTime();
			_timer = new(Tick, null, 0, (int)(TickInterval*1000));
		}
		/// <summary>
		/// Stop the clock
		/// Ticking will stop
		/// </summary>
		public void Stop()
		{
			_timer.Change(Timeout.Infinite, Timeout.Infinite);
			_timer.Dispose();
		}

		/// <summary>
		/// The event arguments for <see cref="OnChangeHandler"/>
		/// </summary>
		public class ChangeEventArgs : EventArgs
		{
			public readonly TimeOnly oldTime;
			readonly public TimeSpan diff;
			public readonly TimeOnly newTime;

			/// <summary>
			/// The previous time the <see cref="Clock"/> held and the new time it has changed time
			/// </summary>
			/// <param name="oldTime">The previous time value</param>
			/// <param name="newTime">The new time value; what the previous time changed to</param>
			public ChangeEventArgs(TimeOnly oldTime, TimeOnly newTime)
			{
				this.oldTime = oldTime;
				this.newTime = newTime;
				diff = newTime - oldTime;
			}
		}
	}
}