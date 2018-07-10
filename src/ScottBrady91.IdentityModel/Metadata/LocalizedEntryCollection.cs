using System.Collections.ObjectModel;
using System.Globalization;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class LocalizedEntryCollection<T> : KeyedCollection<string, T> where T : LocalizedEntry
    {
		protected override string GetKeyForItem(T item)
		{
			return item.Language;
		}
	}
}
