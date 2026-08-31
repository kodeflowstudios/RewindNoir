// ============================================================================
//  Parley - Dialogue System
//  Copyright (c) 2026 KodeFlow Studios. All rights reserved.
// ----------------------------------------------------------------------------
//  File:    FlagHandling.cs
//  Summary: Parley's memory. Dialogue nodes set flags, and later nodes can
//           branch on whether those flags are set. This is how "remember I
//           told you about the dragon" works, mechanically speaking.
// ============================================================================

using System.Collections.Generic;
using KodeFlowStudios.Parley.ErrorHandling;

namespace KodeFlowStudios.Parley.FlagHandling
{
	/// <summary>
	/// A simple set of string flags attached to a conversation. Think of it
	/// as a lightweight state bag — no values, just presence/absence. If you
	/// need to track quantities or structured data, lift it into your own
	/// game state and leave flags for narrative branching.
	/// </summary>
	public class FlagHandler
	{
		private HashSet<string> setFlags = new HashSet<string>();

		/// <summary>Marks a single flag as set. Silently no-ops on null/empty input (and logs an error).</summary>
		public void SetFlag(string flag)
		{
			if (!string.IsNullOrEmpty(flag)) 
			{
				setFlags.Add(flag);
				ErrorHandler.LogDebug($"Flag set: {flag}");
			} 
			else ErrorHandler.ThrowError("FLG01", "No Flag provided!");
		}

		/// <summary>Sets multiple flags in one go. Convenient when a node ends a whole questline.</summary>
		public void SetFlags(List<string> _flags)
		{
			if (_flags == null || _flags.Count == 0)
			{
				ErrorHandler.ThrowError("FLG02", "No Flags to set.");
				return;
			}
			foreach (string flag in _flags) SetFlag(flag);
		}

		/// <summary>True if <paramref name="flag"/> has been set on this conversation.</summary>
		public bool IsFlagSet(string flag) => setFlags.Contains(flag);

		/// <summary>Unsets a single flag. No-ops if it wasn't set to begin with.</summary>
		public void ClearFlag(string flag) => setFlags.Remove(flag);

		/// <summary>Unsets a batch of flags. Useful when a major story beat resets a chapter's state.</summary>
		public void ClearFlags(List<string> _flags)
		{
			if (_flags == null || _flags.Count == 0)
			{
				ErrorHandler.ThrowError("FLG03", "No Flags to clear.");
				return;
			}
			foreach (string flag in _flags) ClearFlag(flag);
		}

		/// <summary>Returns a snapshot copy of every currently-set flag. Safe to iterate or persist.</summary>
		public List<string> GetAllSetFlags() => new List<string>(setFlags);

		/// <summary>Inverts a flag's presence — adds it if absent, removes it if present.</summary>
		public void ToggleFlag(string flag)
		{
			if (!string.IsNullOrEmpty(flag))
			{
				if (setFlags.Contains(flag)) setFlags.Remove(flag);
				else setFlags.Add(flag);
			}
			else ErrorHandler.ThrowError("FLG01", "No Flag provided!");
		}

		/// <summary>Wipes the slate clean. Every flag forgotten. Tabula rasa.</summary>
		public void ClearAllFlags() => setFlags.Clear();
	}
}
