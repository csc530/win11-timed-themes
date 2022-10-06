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
			public int interval;
		}


		TimeOnly Time
		{
			get { return _time; }
			set
			{
				_time = value;
				OnTick?.Invoke(value, generateTickEventArgs());
			}
		}

		public Clock() { };

		public Clock(String time)
		{
			Time = TimeOnly.Parse(time);
		}

		public start() { }
		public stop() { }

		TickEventArgs generateTickEventArgs() { }
	}
}
