using System.ComponentModel;

namespace DatacuteFontConverterUI
{
	public class NamedItem<T> : INotifyPropertyChanged
	{
		private string _name;

		public NamedItem(T item, string name)
		{
			Item = item;
			_name = name;
		}

		public T Item { get; }

		public string Name
		{
			get => _name;
			set {
				if (_name != value)
				{
					_name = value;
					OnPropertyChanged("Name");
				}
			}
		}

		//public override string ToString() => Name;

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		public void Deconstruct(out T Item, out string Name)
		{
			Item = this.Item;
			Name = this.Name;
		}
	}
}