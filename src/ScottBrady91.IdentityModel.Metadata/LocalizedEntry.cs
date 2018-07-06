namespace ScottBrady91.IdentityModel.Metadata
{
    public abstract class LocalizedEntry
    {
        protected LocalizedEntry() { }
        protected LocalizedEntry(string language)
        {
            Language = language;
        }

        public string Language { get; set; }
    }
}