namespace Timed_Theme
{
	/// <summary>
	/// Custom Error codes.
	/// </summary>
	internal enum ErrorCodes
	{
		/// <summary>
		/// No errors; Success
		/// </summary>
		None = 0,
		/// <summary>
		/// The request is not supported.
		/// </summary>
		ErrorBadCommand = 0x16,
		/// <summary>
		/// The device does not recognize the command.
		/// </summary>
		ErrorNotSupported = 0x32,
	}
}