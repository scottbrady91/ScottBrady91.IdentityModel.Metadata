using System.Collections.ObjectModel;
using System.Globalization;

namespace ScottBrady91.IdentityModel.Metadata
{
    public class LocalizedEntryCollection<T> : KeyedCollection<CultureInfo, T> where T : LocalizedEntry
    {
		protected override CultureInfo GetKeyForItem(T item)
		{
			return item.Language;
		}
	}
}
