// ============================================================================
//  Parley - Dialogue System
//  Copyright (c) 2026 KodeFlow Studios. All rights reserved.
// ----------------------------------------------------------------------------
//  File:    Localization.cs
//  Summary: The language table. Every language Parley knows about is
//           registered here, along with helpers to look one up by enum ID,
//           ISO code, or English name. Adding a new language is a two-step
//           job: append to the enum, then add a matching dictionary entry.
// ============================================================================

using System.Linq;
using System.Collections.Generic;
using KodeFlowStudios.Parley.ErrorHandling;

namespace KodeFlowStudios.Parley.Localization
{
	/// <summary>
	/// Strongly-typed identifier for every language Parley supports. Using
	/// an enum instead of raw strings catches typos at compile time — and
	/// makes it much easier to rename a language later without sweating a
	/// find-and-replace across a whole project.
	/// </summary>
	public enum LanguageID
	{
		English,
		German,
		// add more languages here..
	}

	/// <summary>Reading direction for a language.</summary>
	public enum TextDirection { LTR, RTL };

	/// <summary>
	/// Central entry point for language metadata and runtime language state in Parley.
	/// Use it to resolve a <see cref="LanguageID"/> from an ISO code, look up a language's
	/// English name and reading direction, or query the active language at runtime.
	/// All members are static.
	/// </summary>
	public static class Localizer
	{
		/// <summary>Default language used whenever a call doesn't pass one explicitly.</summary>
		public static LanguageID FallbackLanguage = LanguageID.English;

		/// <summary>Default text direction for the fallback language.</summary>
		public static TextDirection FallbackTextDirection = TextDirection.LTR;

		/// <summary>
		/// The master registry that maps every <see cref="LanguageID"/> to its
		/// ISO code, native spelling, and English name. Keep this in sync
		/// with the enum above, or lookups will throw at runtime.
		/// </summary>
		public static readonly Dictionary<LanguageID, LanguageInfo> Languages = new()
		{
			{
				LanguageID.English,
				new LanguageInfo("en", "English", "English")
			},
			{
				LanguageID.German,
				new LanguageInfo("de", "Deutsch", "German")
			},
			// add more languages here..
		};

		/// <summary>
		/// Finds a <see cref="LanguageID"/> by its ISO 639-1 code (e.g. "en", "ar"). Case-insensitive.
		/// </summary>
		public static LanguageID GetIDFromCode(string code)
		{
			if (string.IsNullOrEmpty(code))
			{
				ErrorHandler.ThrowError("LOC01", "Language code is null or empty.");
				return default;
			}

			string normalized = code.ToLowerInvariant();
			foreach (var pair in Languages)
			{
				if (pair.Value.Code == normalized)
					return pair.Key;
			}

			ErrorHandler.ThrowError("LOC02", $"No language registered with ISO code '{code}'.");
			return default;
		}

		/// <summary>
		/// Finds a <see cref="LanguageID"/> by its English name (e.g. "English", "Arabic"). Case-insensitive.
		/// </summary>
		public static LanguageID GetIDFromEnglishName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				ErrorHandler.ThrowError("LOC03", "Language name is null or empty.");
				return default;
			}

			string normalized = name.ToLowerInvariant();
			foreach (var pair in Languages)
			{
				if (pair.Value.EnglishName.ToLowerInvariant() == normalized)
					return pair.Key;
			}

			ErrorHandler.ThrowError("LOC04", $"No language registered with English name '{name}'.");
			return default;
		}

		/// <summary>Returns the full <see cref="LanguageInfo"/> for a given <see cref="LanguageID"/>.</summary>
		public static LanguageInfo GetInfoFromID(LanguageID language)
		{
			if (!Languages.TryGetValue(language, out var info))
			{
				ErrorHandler.ThrowError("LOC05", $"No info registered for language '{language}'. Did you extend LanguageID without adding a matching entry to Languages?");
				return default;
			}
			return info;
		}

		/// <summary>Returns the full <see cref="LanguageInfo"/> for a given ISO 639-1 code (e.g. "en"). Case-insensitive.</summary>
		public static LanguageInfo GetInfoFromCode(string code)
			=> Languages[GetIDFromCode(code)];
	}

	/// <summary>
	/// Small immutable bundle of the three things we want to know about any
	/// language: its ISO code (for file paths), its native name (for UI),
	/// and its English name (for editor tooling and folder names).
	/// </summary>
	public readonly struct LanguageInfo
	{
		public readonly string Code;
		public readonly string NativeName;
		public readonly string EnglishName;

		public LanguageInfo(string code, string native, string english)
		{
			Code = code;
			NativeName = native;
			EnglishName = english;
		}
	}
}
