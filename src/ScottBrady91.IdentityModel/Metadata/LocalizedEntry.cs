using System;
using System.Globalization;

namespace ScottBrady91.IdentityModel.Metadata
{
    public abstract class LocalizedEntry
    {
		public string Language { get; set; }

		protected LocalizedEntry()
		{
		}

		protected LocalizedEntry(string language)
		{
			Language = language;
		}
	}
}
