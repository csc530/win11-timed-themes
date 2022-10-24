namespace Timed_Theme
{           //! utilities include same line writing, prompt, alignments

	internal class Writer
	{
		public Stream Output { get; set; }

		public Writer(Stream output)
		{
			Output = output;
		}
	}
}
