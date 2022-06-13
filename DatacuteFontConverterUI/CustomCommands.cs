
using System.Windows.Input;

namespace DatacuteFontConverterUI
{
	public static class CustomCommands
	{
		public static readonly RoutedUICommand Exit = new RoutedUICommand
		(
			"Exit",
			"Exit",
			typeof(CustomCommands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.F4, ModifierKeys.Alt)
			}
		);

		public static readonly RoutedUICommand Include = new RoutedUICommand
		(
			"Include",
			"Include",
			typeof(CustomCommands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.Add, ModifierKeys.Control)
			}
		);

		public static readonly RoutedUICommand Combine = new RoutedUICommand
		(
			"Combine",
			"Combine",
			typeof(CustomCommands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.Multiply, ModifierKeys.Control)
			}
		);

		public static readonly RoutedUICommand Sample = new RoutedUICommand
		(
			"Sample",
			"Sample",
			typeof(CustomCommands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.Enter, ModifierKeys.Alt)
			}
		);

		public static readonly RoutedUICommand TypefaceInfo = new RoutedUICommand
		(
			"Info",
			"TypefaceInfo",
			typeof(CustomCommands),
			new InputGestureCollection()
			{
				new KeyGesture(Key.F1, ModifierKeys.None)
			}
		);

	}}