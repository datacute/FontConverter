using System;
using System.Collections.ObjectModel;

namespace DatacuteFontConverterUI
{
	public class NamedCollection<T> : ObservableCollection<NamedItem<T>>
	{
		private NamedItemFactory<T> _namedItemFactory;

		public NamedCollection(Func<T, string> namingFunc)
		{
			_namedItemFactory = new NamedItemFactory<T>(namingFunc);
		}

		public NamedCollection(NamedItemFactory<T> namedItemFactory)
		{
			_namedItemFactory = namedItemFactory;
		}

		public void RenameAll(Func<T, string> namingFunc)
		{
			_namedItemFactory = new NamedItemFactory<T>(namingFunc);
			foreach (var namedItem in this)
			{
				namedItem.Name = namingFunc(namedItem.Item);
			}
		}

		public void Add(T item)
		{
			Add(_namedItemFactory.Create(item));
		}
	}
}