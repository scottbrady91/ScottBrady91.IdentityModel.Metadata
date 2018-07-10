using System.Globalization;

namespace ScottBrady91.IdentityModel.Metadata
{
    public abstract class LocalizedEntry
    {
		public CultureInfo Language { get; set; }

		protected LocalizedEntry() { }
		protected LocalizedEntry(CultureInfo language)
		{
			Language = language;
		}
	}
}
