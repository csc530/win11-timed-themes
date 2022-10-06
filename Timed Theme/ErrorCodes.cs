using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timed_Theme
{
	internal static class ErrorCodes
	{

		public const int NONE = 0;
		// 	//The request is not supported.
		public const int ERROR_BAD_COMMAND = 0x16;
		// The device does not recognize the command.
		public const int ERROR_NOT_SUPPORTED = 0x32;

		/*todo consider adding somehow an auto convert type in this error class with an enum and overloader methods??? maybe*/
	}
}
