using System.Globalization;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class LocalizedName : LocalizedEntry
    {
		public string Name { get; set; }

        public LocalizedName() { }
        public LocalizedName(string name, CultureInfo language) : base(language)
		{
			Name = name;
		}

    }
}
