// ============================================================================
//  Parley - Dialogue System
//  Copyright (c) 2026 KodeFlow Studios. All rights reserved.
// ----------------------------------------------------------------------------
//  File:    Deserialization.cs
//  Summary: Handles the ugly bits of turning YAML files on disk (or baked
//           TextAssets inside Resources/) into strongly-typed C# objects.
//           Everything here is static and stateless — one deserializer is
//           built once, then reused for the lifetime of the app.
// ============================================================================

using System;
using UnityEngine;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using KodeFlowStudios.Parley.Localization;
using KodeFlowStudios.Parley.ErrorHandling;

namespace KodeFlowStudios.Parley.Deserialization
{
	/// Loads and parses Parley YAML fileNames from disk or Resources.
	/// Also resolves the canonical path layout that the rest of the system
	/// expects (<c>Resources/{languageEnglishName}/{folderName}/{lang}.{fileName}</c>).
	internal static class Deserializer
	{
		private static IDeserializer _deserializer;

		static Deserializer()
		{
			_deserializer = new DeserializerBuilder()
				.WithNamingConvention(PascalCaseNamingConvention.Instance)
				.IgnoreUnmatchedProperties()
				.Build();
		}

		public static T LoadFromFile<T>(string filePath) where T : class
		{
			ErrorHandler.LogDebug($"Loading YAML from: {filePath}");
			string yamlContent = LoadYamlFile(filePath);
			if (yamlContent == null)
			{
				ErrorHandler.ThrowError("DSR01", "YAML content is empty.");
				return null;
			}

			return Deserialize<T>(yamlContent);
		}

		// Builds the on-disk YAML path for a fileName.
		public static string GetYamlPath(string folderName, string fileName, string languageCode) { return GetPath(folderName, fileName, languageCode) + ".yaml"; }

		public static string GetGraphPath(string folderName, string fileName, string languageCode) { return GetPath(folderName, fileName, languageCode).Substring("Resources/".Length); }

		// Single source of truth for the on-disk layout:
		// Resources/{languageEnglishName}/{folderName}/{langCode}.{fileName}
		// Folder uses the readable English name; filename prefix uses the ISO code.
		// Change here to evolve the layout — all path logic funnels through this.
		private static string GetPath(string folderName, string fileName, string languageCode)
		{
			return $"Resources/{Localizer.GetInfoFromCode(languageCode).EnglishName}/{folderName}/{languageCode}.{fileName}";
		}

		public static T Deserialize<T>(string yamlContent) where T : class
		{
			if (string.IsNullOrEmpty(yamlContent))
			{
				ErrorHandler.ThrowError("DSR02", "YAML content is null or empty.");
				return null;
			}

			try
			{
				return _deserializer.Deserialize<T>(yamlContent);
			}
			catch (Exception ex)
			{
				ErrorHandler.ThrowError("DSR03", $"Failed to deserialize YAML: {ex.Message}");
				return null;
			}
		}

		public static string LoadYamlFile(string filePath)
		{
			// Resources path: let Unity handle it. This is the path that
			// actually works in a built player, since Resources files get
			// packed into the build and File.ReadAllText won't find them.
			if (filePath.StartsWith("Resources/"))
			{
				string resourcePath = filePath.Substring("Resources/".Length);
				resourcePath = resourcePath.Replace(".yaml", "").Replace(".yml", "");

				TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
				if (textAsset == null)
				{
					ErrorHandler.ThrowError("DSR04", $"Resource not found: {resourcePath}.");
					return null;
				}
				return textAsset.text;
			}
#if UNITY_EDITOR
			// Direct filesystem reads are editor-only. Built Players use "Resources/"
			if (!File.Exists(filePath))
			{
				ErrorHandler.ThrowError("DSR05", $"File doesn't exist: {filePath}.");
				return null;
			}

			try
			{
				return File.ReadAllText(filePath);
			}
			catch (Exception ex)
			{
				ErrorHandler.ThrowError("DSR06", $"Failed to read file {filePath}: {ex.Message}");
				return null;
			}
#else
			ErrorHandler.ThrowError("DSR07", $"Non-Resources path '{filePath}' is not supported in built players. Place the file under Resources/ or load via TextAsset.");
			return null;
#endif
		}
	}
}
