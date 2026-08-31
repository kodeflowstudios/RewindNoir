// ============================================================================
//  Parley - Dialogue System
//  Copyright (c) 2026 KodeFlow Studios. All rights reserved.
// ----------------------------------------------------------------------------
//  File:    YamlCore.cs
//  Summary: The YAML-first counterpart to GraphCore. Instead of a baked
//           ScriptableObject, this runs conversations straight off a
//           human-authored .yaml file — great for writers who like to live
//           in text editors, or for teams that want to hot-swap dialogue
//           without touching the editor.
// ============================================================================

using System;
using System.Linq;
#if HAS_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using System.Collections.Generic;
using System.Threading.Tasks;
using KodeFlowStudios.Parley.Localization;
using KodeFlowStudios.Parley.Deserialization;
using KodeFlowStudios.Parley.EventHandling;
using KodeFlowStudios.Parley.ErrorHandling;
using KodeFlowStudios.Parley.FlagHandling;

namespace KodeFlowStudios.Parley.YamlCore
{
	/// <summary>
	/// The YAML Conversation Engine for Parley.
	/// </summary>
	public class ParleyYaml
	{
		private TaskCompletionSource<int> _choiceSelectionSource;

#if HAS_INPUT_SYSTEM
		private InputAction _nextDialogue;
		private Action<InputAction.CallbackContext> _nextHandler;
#endif

		/// <summary>Fires when the player advances. Await or subscribe as you like.</summary>
		public AwaitableEvent<string> OnNextDialogue { get; private set; } = new AwaitableEvent<string>();

		public bool ConversationEnded = false;

		// The parsed node table keyed by node name. YAML authoring uses
		// readable IDs like "node_1", "ask_about_the_dragon", etc.
		private Dictionary<string, DialogueNode> _allNodes;

		/// <summary>The node currently selected. Null once the conversation ends.</summary>
		public DialogueNode CurrentNode { get; private set; }

		public FlagHandler Flags = new();

		public MetaData Meta;

		// Alias table from the YAML - resolved lazily on each visible field.
		private Aliases _aliases;

		/// <summary>Language this instance was loaded in.</summary>
		public LanguageID CurrentLanguage = Localizer.FallbackLanguage;

		/// <summary>
		/// Loads and starts a YAML-backed conversation. Same contract as
		/// <see cref="GraphCore.ParleyGraph"/>: construct, hook up events,
		/// then drive it with player input.
		/// </summary>
		/// <param name="folderName">Folder name — matches the sub-folder under the language folder.</param>
		/// <param name="fileName">The conversation file name (without the language prefix).</param>
		/// <param name="languageID">Which language to load. Defaults to <see cref="Localizer.FallbackLanguage"/>.</param>
		public ParleyYaml(string folderName, string fileName, LanguageID? languageID = null)
		{
			languageID ??= Localizer.FallbackLanguage;
			CurrentLanguage = (LanguageID)languageID;

#if HAS_INPUT_SYSTEM
			_nextDialogue = InputSystem.actions.FindAction("NextDialogue") ?? InputSystem.actions.FindAction("NextDialog");

			if (_nextDialogue == null)
			{
				ErrorHandler.LogWarning("NextDialogue/NextDialog action not found. Falling back to left mouse click.");
				_nextDialogue = new InputAction("NextDialogue", binding: "<Mouse>/leftButton");
				_nextDialogue.Enable();
			}

			BindNextEvent(_nextDialogue);
			OnNextDialogue += ProgressDialogue;
			LoadConversation(folderName, fileName);
#endif
		}

#if HAS_INPUT_SYSTEM
		/// <summary>Rebinds the "advance" signal to a different input action.</summary>
		public void BindNextEvent(InputAction inputAction)
		{
			UnBindNextEvent();
			inputAction.Enable();
			_nextDialogue = inputAction;
			_nextHandler = ctx => OnNextDialogue?.Invoke("*");
			_nextDialogue.performed += _nextHandler;
		}

		/// <summary>Detaches the input handler. Called automatically on end.</summary>
		public void UnBindNextEvent()
		{
			if (_nextDialogue != null && _nextHandler != null)
			{
				_nextDialogue.performed -= _nextHandler;
				_nextDialogue = null;
				_nextHandler = null;
			}
		}
#endif

		// ------------------------------------------------------------------
		//  YAML-shape models. These are deliberately separate from the
		//  RuntimeDialogueGraph/Node types because YAML's preferred layout is
		//  slightly different (nested dictionaries, named keys, true/false
		//  branch fields) and we don't want the two to drift-couple.
		// ------------------------------------------------------------------

		/// <summary>Top-level shape of a Parley YAML file.</summary>
		internal class ParleyData
		{
			public MetaData MetaData { get; set; }
			public Aliases Aliases { get; set; }
			public Dictionary<string, DialogueNode> Dialogue { get; set; }
		}

		public class MetaData
		{
			public LanguageID Language { get; set; }
			public TextDirection TextDirection { get; set; }
		}

		internal class Aliases : Dictionary<string, Dictionary<string, string>> { }

		/// <summary>
		/// A single node in YAML form. Dialogue, choice and utility-ish
		/// branching nodes are all represented by this one type — whichever
		/// fields are populated determines what the node does at runtime.
		/// </summary>
		public class DialogueNode
		{
			public string Speaker { get; set; }
			public string Emotion { get; set; }
			public string Text { get; set; }
			public List<ChoiceData> Choices { get; set; }
			public string NextNode { get; set; }
			public List<string> SetFlags { get; set; }

			// Branching fields: if CheckFlags is non-empty the node acts as
			// a utility branch — TrueNode is taken when every listed flag
			// is set, FalseNode otherwise.
			public List<string> CheckFlags { get; set; }
			public string TrueNode { get; set; }
			public string FalseNode { get; set; }
		}

		public class ChoiceData
		{
			public string Text { get; set; }
			public List<string> SetFlags { get; set; }
			public string NextNode { get; set; }
		}

		private string ResolveAliases(string text)
		{
			if (string.IsNullOrEmpty(text) || _aliases == null || _aliases.Count == 0) return text;
			foreach (var entry in _aliases)
				foreach (var field in entry.Value)
				{
					string token = $"@{entry.Key}.{field.Key}";
					text = text.Replace(token, field.Value, StringComparison.OrdinalIgnoreCase);
				}
			return text;
		}

		private DialogueNode ResolveNodeAliases(DialogueNode node)
		{
			if (node == null || _aliases == null || _aliases.Count == 0) return node;
			node.Speaker = ResolveAliases(node.Speaker);
			node.Emotion = ResolveAliases(node.Emotion);
			node.Text = ResolveAliases(node.Text);
			if (node.Choices != null)
				foreach (var choice in node.Choices)
					choice.Text = ResolveAliases(choice.Text);
			return node;
		}

		// Pulls the YAML off disk, unpacks it into the runtime fields, and
		// starts on "node_1" by convention. If that node is missing we bail
		// quietly. Authors see the error in the Console, players see nothing.
		private void LoadConversation(string folderName, string fileName)
		{
			ParleyData parleyData = Deserializer.LoadFromFile<ParleyData>(
				Deserializer.GetYamlPath(folderName, fileName, Localizer.GetInfoFromID(CurrentLanguage).Code)
			);

			Meta = parleyData.MetaData;
			_aliases = parleyData.Aliases;
			_allNodes = parleyData.Dialogue;

			if (_allNodes == null)
			{
				ErrorHandler.LogWarning("Failed to load conversation: node_1 not found.");
				return;
			}
			CurrentNode = _allNodes.Values.FirstOrDefault();
			ResolveNodeAliases(CurrentNode);
		}

		/// <summary>Returns the requested node if it exists.</summary>
		public DialogueNode GetNode(string node)
		{
			if (string.IsNullOrEmpty(node))
			{
				ErrorHandler.ThrowError("MSC01", "No node name provided.");
				return null;
			}

			if (!_allNodes.TryGetValue(node, out DialogueNode returnedNode))
			{
				ErrorHandler.ThrowError("MSC02", "No nodes matching provided node.");
				return null;
			}

			return returnedNode;
		}

		/// <summary>Returns the choices available on the current node, or an empty list if there aren't any.</summary>
		public List<ChoiceData> GetCurrentChoices()
		{
			if (CurrentNode == null || CurrentNode.Choices == null) return new();
			return CurrentNode.Choices;
		}

		/// <summary>
		/// Advances the conversation one step. Pass <c>"*"</c> to follow the
		/// current node's <c>NextNode</c>, or a specific node ID to jump.
		/// Handles flag-check branching inline — if the current node has
		/// <c>CheckFlags</c>, they're evaluated and the True/False branch
		/// is chosen before we move.
		/// </summary>
		public void ProgressDialogue(string nextNode = "*")
		{
			if (ConversationEnded) return;
			if (CurrentNode == null) { EndDialogue(); return; }
			if (GetCurrentChoices().Count > 0) return;

			// Resolve the next node ID from CheckFlags or NextNode
			if (nextNode == "*") nextNode = (CurrentNode.CheckFlags?.Count > 0)
											? (CurrentNode.CheckFlags.All(f => Flags.IsFlagSet(f)) ? CurrentNode.TrueNode : CurrentNode.FalseNode)
											: CurrentNode.NextNode;

			if (string.IsNullOrEmpty(nextNode) || _allNodes == null || !_allNodes.ContainsKey(nextNode))
			{
				EndDialogue();
				return;
			}

			if (!_allNodes.TryGetValue(nextNode, out DialogueNode returnedNode))
			{
				ErrorHandler.ThrowError("MSC02", "No nodes matching provided node.");
			}

			CurrentNode = returnedNode;
			ResolveNodeAliases(CurrentNode);

			if (CurrentNode.SetFlags != null)
			{
				foreach (string flag in CurrentNode.SetFlags)
				{
					Flags.SetFlag(flag);
				}
			}

			// Utility node, no visible content or pure flag-check branch; resolve transparently
			bool isUtility = (CurrentNode.CheckFlags?.Count > 0)
				|| (string.IsNullOrEmpty(CurrentNode.Speaker) && string.IsNullOrEmpty(CurrentNode.Text));

			if (isUtility)
			{
				ProgressDialogue();
				return;
			}
		}

		/// <summary>
		/// Records a choice pick and follows that choice's branch. Invalid
		/// indices and missing target nodes are handled gracefully, bad
		/// input ends the conversation rather than throwing.
		/// </summary>
		public void ChoiceMade(int index)
		{
			if (ConversationEnded) return;
			var choices = GetCurrentChoices();
			if (index < 0 || index >= choices.Count) return;

			ErrorHandler.LogDebug($"Choice {index} selected.");

			string nextNodeId = choices[index].NextNode;
			if (string.IsNullOrEmpty(nextNodeId) || _allNodes == null || !_allNodes.ContainsKey(nextNodeId))
			{
				ErrorHandler.LogWarning($"Choice target node '{nextNodeId}' not found.");
				EndDialogue();
				return;
			}

			// Set flags from the choice itself
			if (choices[index].SetFlags != null)
				foreach (string flag in choices[index].SetFlags)
				{
					Flags.SetFlag(flag);
					ErrorHandler.LogDebug($"Flag set: {flag}");
				}

			// Move to the target node and let ProgressDialogue handle the rest
			CurrentNode = _allNodes[nextNodeId];
			ResolveNodeAliases(CurrentNode);

			if (CurrentNode.SetFlags != null)
				foreach (string flag in CurrentNode.SetFlags)
				{
					Flags.SetFlag(flag);
					ErrorHandler.LogDebug($"Flag set: {flag}");
				}

			bool isUtility = (CurrentNode.CheckFlags?.Count > 0)
				|| (string.IsNullOrEmpty(CurrentNode.Speaker) && string.IsNullOrEmpty(CurrentNode.Text));
			if (isUtility)
				ProgressDialogue();

			// Fire after chain fully resolves so UI wakes up with the correct CurrentNode
			var source = _choiceSelectionSource;
			_choiceSelectionSource = null;
			source?.SetResult(index);
		}

		/// <summary>
		/// Awaitable choice input. Throws if called while no choices are
		/// available — that's almost always a sign the gameplay state and
		/// dialogue state have drifted out of sync, and we'd rather fail
		/// loudly than silently hang on a task that will never complete.
		/// </summary>
		public async Task<int> GetPlayerChoice()
		{
			var choices = GetCurrentChoices();
			if (choices.Count == 0)
				throw new InvalidOperationException("Cannot get player choice when no choices are available");

			_choiceSelectionSource = new TaskCompletionSource<int>();
			return await _choiceSelectionSource.Task;
		}

		/// <summary>Ends the conversation early: detaches input, flips the flag, and leaves the stage.</summary>
		public void EndDialogue()
		{
			ErrorHandler.LogDebug("Conversation ended.");
#if HAS_INPUT_SYSTEM
			UnBindNextEvent();
#endif
			ConversationEnded = true;
		}
	}
}
