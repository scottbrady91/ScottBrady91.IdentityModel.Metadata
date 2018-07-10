using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class LocalizedName : LocalizedEntry
    {
		public string Name { get; set; }

		public LocalizedName(string name, string language) :
			base(language)
		{
			Name = name;
		}

		public LocalizedName()
		{
		}
    }
}
