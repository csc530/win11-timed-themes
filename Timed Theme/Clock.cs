using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timed_Theme
{

	internal class Clock
	{
		private TimeOnly _time = TimeOnly.MinValue;

		public event EventHandler<TickEventArgs>? OnTickHandler;
		protected virtual void OnTick() => OnTickHandler?.Invoke(this, generateTickEventArgs());
		public int tickInterval { get; set; }//make setter
		public class TickEventArgs : EventArgs
		{
			readonly public int interval;
		}

		public EventHandler<ChangeEventArgs>? OnChangeHandler { get; set; }
		protected virtual void OnChange(TimeOnly old) => OnChangeHandler?.Invoke(this, generateChangeEventArgs(old));
		TimeOnly Time
		{
			get { return _time; }
			set
			{
				var prev = _time;
				_time = value;
				OnChange(prev);
			}
		}

		public Clock() { }

		public Clock(String time)
		{
			Time = TimeOnly.Parse(time);
		}

		//todo add increment and decrement methods
		public void start() { }
		public void stop() { }

		TickEventArgs generateTickEventArgs()
		{
			return new TickEventArgs();
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
}
