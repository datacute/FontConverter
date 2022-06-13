using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DatacuteFontConverterUI.ConversionRights;
using static DatacuteFontConverterUI.ConversionRights.CheckedItem;
using static DatacuteFontConverterUI.ConversionRights.CheckStyle;
using static DatacuteFontConverterUI.ConversionRights.Rights;

namespace DatacuteFontConverterUI
{
	public class UserSettings
	{
		public const string DefaultFileName = "UserSettings.json";
		public const string AppDataDirectoryName = "DatacuteFontConverter";
		public static UserSettings Default => new()
		{
			SampleText = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\] ^_ `abcdefghijklmnopqrstuvwxyz{|}~",
			EmSize = 16d,
			PreviewScale = 2.0d,
			DesiredCodepointRanges = new CodepointRangeProperty[] { new('!', '~') },
			ConversionRights = DefaultRights,
			ReservedFontNames = DefaultReservedFontNames
		};

		public static readonly List<RightsCheck> DefaultRights = new()
		{
			new RightsCheck(License, StartsWith, "Microsoft supplied font.", ConversionRestricted),
			new RightsCheck(License, Exactly, "Please contact the vendor to learn more about license restrictions.", ConversionRestricted),
			new RightsCheck(Copyright, Contains, "The Monotype Corporation", ConversionRestricted),
			new RightsCheck(Copyright, Exactly, "Copyright (c) Font Awesome", ConversionAllowed),
			new RightsCheck(Copyright, Exactly, "Copyright 2018 Google, Inc. All Rights Reserved.", ConversionAllowed),
			new RightsCheck(Copyright, Exactly, "Copyright 2019 Google LLC. All Rights Reserved.", ConversionAllowed),
			new RightsCheck(License, Contains, "SIL Open Font License", ConversionAllowed | CheckReservedFontNames),
			new RightsCheck(License, Contains, "Apache License", ConversionAllowed),
		};

		public static readonly Dictionary<string, List<string>> DefaultReservedFontNames = new()
		{
			{"Unifont", new List<string> {"Unifont"}}, // Unifont is dual licensed, with a copyright statement containing the reserved font names in the license description.
		};

		public string SaveDirectory { get; set; }
		public string SampleText { get; set; }
		public double EmSize { get; set; }
		public double PreviewScale { get; set; }
		public CodepointRangeProperty[] DesiredCodepointRanges { get; set; }

		public List<RightsCheck> ConversionRights { get; set; }

		public Dictionary<string, List<string>> ReservedFontNames { get; set; }

		public static UserSettings LoadUserSettings()
		{
			var userSettingsFile = UserSettingsFile();
			bool upgraded = false;
			UserSettings userSettings;
			if (userSettingsFile.Exists)
			{
				var jsonUtf8Bytes = File.ReadAllBytes(userSettingsFile.FullName);
				var readOnlySpan = new ReadOnlySpan<byte>(jsonUtf8Bytes);
				userSettings = JsonSerializer.Deserialize<UserSettings>(readOnlySpan) ?? Default;
				if (userSettings.ConversionRights == null || userSettings.ConversionRights.Count == 0)
				{
					userSettings.ConversionRights = DefaultRights;
					upgraded = true;
				}
				if (userSettings.ReservedFontNames == null || userSettings.ReservedFontNames.Count == 0)
				{
					userSettings.ReservedFontNames = DefaultReservedFontNames;
					upgraded = true;
				}
			}
			else
			{
				userSettings = Default;
				upgraded = true;
			}
			if (upgraded)
				userSettings.Save();

			return userSettings;
		}


		public void Save()
		{
			var userSettingsFile = UserSettingsFile();
			byte[] jsonUtf8Bytes =
				JsonSerializer.SerializeToUtf8Bytes(this, new JsonSerializerOptions
				{
					WriteIndented = true,
					IgnoreReadOnlyProperties = true
				});
			File.WriteAllBytes(userSettingsFile.FullName, jsonUtf8Bytes);
		}

		private static FileInfo UserSettingsFile()
		{
			var appDataDirectory = AppDataDirectory();
			var userSettingsFile = new FileInfo(Path.Combine(appDataDirectory.FullName, DefaultFileName));
			return userSettingsFile;
		}

		private static DirectoryInfo AppDataDirectory()
		{
			string appDataPath =
				Path.Combine(
					Environment.GetFolderPath(
						Environment.SpecialFolder.LocalApplicationData,
						Environment.SpecialFolderOption.DoNotVerify),
					AppDataDirectoryName);
			var appDataDirectory = Directory.CreateDirectory(appDataPath);
			return appDataDirectory;
		}
	}
}