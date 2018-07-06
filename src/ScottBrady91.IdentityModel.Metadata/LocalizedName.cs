namespace ScottBrady91.IdentityModel.Metadata
{
    public class LocalizedName : LocalizedEntry
    {
        public LocalizedName() { }
        public LocalizedName(string name, string language) : base(language)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}