using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DatacuteFontConverterUI.ConversionRights;

namespace DatacuteFontConverterUI
{
	public partial class RightsEditor : Window
	{
		private readonly UserSettings _userSettings;

		public ObservableCollection<RightsCheck> RightsCheckItems { get; } = new();

		public RightsEditor(UserSettings userSettings)
		{
			_userSettings = userSettings;

			foreach (var check in userSettings.ConversionRights)
			{
				RightsCheckItems.Add(check);
			}
			
			InitializeComponent();
			this.DataContext = this;
		}

		public Typeface CurrentTypeface { get; set; }
		public string CurrentCopyrightHash { get; set; }
		public string CurrentLicenseDescriptionsHash { get; set; }

		private void ItemGroupChecked(object sender, RoutedEventArgs e)
		{
			var li = sender as RadioButton;
			if (li is not { IsChecked: true, DataContext: RightsCheck rightsCheck }) return;

			rightsCheck.Item = li.Name switch
			{
				"ItemRadioCopyright" => CheckedItem.Copyright,
				"ItemRadioLicense" => CheckedItem.License,
				"ItemRadioFamily" => CheckedItem.FamilyName,
				_ => rightsCheck.Item
			};
		}

		private void StyleGroupChecked(object sender, RoutedEventArgs e)
		{
			var li = sender as RadioButton;
			if (li is not { IsChecked: true, DataContext: RightsCheck rightsCheck }) return;

			rightsCheck.Style = li.Name switch
			{
				"StyleRadioExactly" => CheckStyle.Exactly,
				"StyleRadioStartsWith" => CheckStyle.StartsWith,
				"StyleRadioContains" => CheckStyle.Contains,
				"StyleRadioExactHash" => CheckStyle.ExactHash,
				_ => rightsCheck.Style
			};
		}
	}
}