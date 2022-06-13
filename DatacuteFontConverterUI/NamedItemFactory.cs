using System;
using System.ComponentModel;

namespace DatacuteFontConverterUI
{
	public class NamedItemFactory<T>
	{
		private readonly Func<T, string> _nameProvider;

		public NamedItemFactory(Func<T, string> nameProvider)
		{
			_nameProvider = nameProvider;
		}

		public NamedItem<T> Create(T item)
		{
			NamedItem<T> namedItem = new(item, _nameProvider(item));
			if (item is INotifyPropertyChanged notifyingItem)
			{
				notifyingItem.PropertyChanged += (sender, args) => namedItem.Name = _nameProvider(item);
			}

			return namedItem;
		}
	}
}