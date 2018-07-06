namespace ScottBrady91.IdentityModel.Metadata
{
    public class Organization
    {
        public LocalizedEntryCollection<LocalizedName> Names { get; } = new LocalizedEntryCollection<LocalizedName>();
        public LocalizedEntryCollection<LocalizedName> DisplayNames { get; } = new LocalizedEntryCollection<LocalizedName>();
        public LocalizedEntryCollection<LocalizedUri> Urls { get; } = new LocalizedEntryCollection<LocalizedUri>();
        
        //public ICollection<XmlElement> Extensions { get; } = new Collection<XmlElement>();
    }
}