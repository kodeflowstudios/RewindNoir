// ============================================================================
//  Parley - Dialogue System
//  Copyright (c) 2026 KodeFlow Studios. All rights reserved.
// ----------------------------------------------------------------------------
//  File:    ErrorHandling.cs
//  Summary: A tiny, opinionated logger that wraps UnityEngine.Debug so every
//           message Parley emits comes out with a consistent prefix — makes
//           it easy to filter the Console for a code when things go wrong.
// ============================================================================

using UnityEngine;

namespace KodeFlowStudios.Parley.ErrorHandling
{
	internal static class ErrorHandler
	{
		static readonly bool debug = false;

		/// <summary>
		/// Logs a red error with a searchable code so users can look it up
		/// in the Parley docs for details and a fix.
		/// Format: <c>Error [CODE]: message</c>
		/// </summary>
		public static string ThrowError(string code, string message)
		{
			Debug.LogError($"Error [{code}]: {message}");
			return code;
		}

		/// <summary>Logs a yellow warning — plain message, no code needed.</summary>
		public static void LogWarning(string message)
		{
			if (!debug) return;
			Debug.LogWarning(message);
		}

		/// <summary>Verbose trace for anything that isn't quite an error or a warning — progress, state, etc. Stripped in production via the debug flag.</summary>
		public static void LogDebug(string message)
		{
			if (!debug) return;
			Debug.Log(message);
		}
	}
}
