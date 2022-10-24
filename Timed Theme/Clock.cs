﻿namespace Timed_Theme
{
	internal class Clock
	{
		private TimeOnly _time = TimeOnly.MinValue;
		private Timer _timer;

		private Dictionary<TimeOnly, EventHandler<TimeOnly>> Alarms;

		public event EventHandler<TickEventArgs>? OnTickHandler;
		// * made it compatible with TimerCallback
		protected virtual void OnTick(object? state) => OnTickHandler?.Invoke(this, generateTickEventArgs());
		private TimeSpan _secondSpan = TimeSpan.FromSeconds(1);
		protected void Tick(object? state)
		{
			Time = Time.Add(_secondSpan);
			OnTick(state ?? this);
		}
		public int TickInterval { get; set; }//make setter
		public class TickEventArgs : EventArgs
		{
			readonly public int elapsedTime;
			readonly public TimeOnly time;
			public TickEventArgs(int interval, TimeOnly time)
			{
				this.elapsedTime = interval;
				this.time = time;
			}
		}

		public EventHandler<ChangeEventArgs>? OnChangeHandler { get; set; }
		protected virtual void OnChange(TimeOnly old) => OnChangeHandler?.Invoke(this, generateChangeEventArgs(old));
		public TimeOnly Time
		{
			get => _time;
			set
			{
				TimeOnly prev = _time;
				_time = value;
				OnChange(prev);
				Alarms.TryGetValue(_time, out var alarm);
				alarm?.Invoke(this, _time);
			}
		}

		#region Constructors

		public Clock()
		{
			_timer = new(OnTick, null, Timeout.Infinite, Timeout.Infinite);
			Alarms = new();
			TickInterval = 1;
		}

		public Clock(String time) : this()
		{
			Time = TimeOnly.Parse(time);
		}

		public Clock(int hour, int minute, int second)
		{
			Time = new(hour, minute, second);
		}

		public Clock(int hour, int minute)
		{
			Time = new TimeOnly(hour, minute);
		}

		public Clock(long ticks)
		{
			Time = new TimeOnly(ticks);
		}

		#endregion

		#region increment
		public int increment(int span, ClockUnit unit)
		{
			int wrappedDays = 0;
			switch(unit)
			{
				case ClockUnit.Hours:
					Time = Time.AddHours(span, out wrappedDays);
					break;
				case ClockUnit.Minutes:
					Time = Time.AddMinutes(span, out wrappedDays);
					break;
				case ClockUnit.Seconds:
					TimeSpan seconds = TimeSpan.FromSeconds(span);
					wrappedDays = increment(seconds);
					break;
				default:
					break;
			}
			return wrappedDays;
		}

		public int increment(TimeSpan span)
		{
			int wrappedDays;
			Time = Time.Add(span, out wrappedDays);
			return wrappedDays;
		}
		#endregion

		#region decrement
		public int decrement(int span, ClockUnit unit)
		{
			int wrappedDays = 0;
			switch(unit)
			{
				case ClockUnit.Hours:
					Time = Time.AddHours(-span, out wrappedDays);
					break;
				case ClockUnit.Minutes:
					Time = Time.AddMinutes(-span, out wrappedDays);
					break;
				case ClockUnit.Seconds:
					TimeSpan seconds = TimeSpan.FromSeconds(-span);
					wrappedDays = decrement(seconds);
					break;
				default:
					break;
			}
			return wrappedDays;
		}

		public int decrement(TimeSpan span)
		{
			int wrappedDays;
			Time = Time.Add(-span, out wrappedDays);
			return wrappedDays;
		}
		#endregion

		#region toString
		public override string ToString()
		{
			return Time.ToString();
		}

		public string ToString(string format)
		{
			return Time.ToString(format);
		}

		public string ToLongString()
		{
			return Time.ToLongTimeString();
		}
		#endregion

		#region Alarms
		public void AddAlarm(TimeOnly time, Action<TimeOnly> action)
		{
			Alarms.Add(time, (sender, time) => action(time));
		}

		public void RemoveAlarm(TimeOnly time)
		{
			Alarms.Remove(time);
		}

		public void ClearAlarms()
		{
			Alarms.Clear();
		}

		public List<TimeOnly> GetAlarms()
		{
			return Alarms.Keys.ToList();
		}

		#endregion

		public void SyncTime()
		{
			Time = TimeOnly.FromDateTime(DateTime.Now);
		}

		public void start()
		{
			SyncTime();
			_timer = new(Tick, null, 0, TickInterval * 1000);
		}
		public void stop()
		{
			_timer.Change(Timeout.Infinite, Timeout.Infinite);
			_timer.Dispose();
		}

		TickEventArgs generateTickEventArgs()
		{
			return new TickEventArgs(TickInterval, Time);
		}
		ChangeEventArgs generateChangeEventArgs(TimeOnly prevTime)
		{
			return new ChangeEventArgs(prevTime, Time);
		}

		public class ChangeEventArgs : EventArgs
		{
			public readonly TimeOnly oldTime;
			readonly public TimeSpan diff;
			public readonly TimeOnly newTime;

			public ChangeEventArgs(TimeOnly oldTime, TimeOnly newTime)
			{
				this.oldTime = oldTime;
				this.newTime = newTime;
				diff = newTime - oldTime;
			}
		}
	}

	public enum ClockUnit
	{
		Hours,
		Minutes,
		Seconds
	}
}
