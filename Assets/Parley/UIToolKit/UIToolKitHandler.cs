// ============================================================================
//  Parley - Dialogue System
//  Copyright (c) 2026 KodeFlow Studios. All rights reserved.
// ----------------------------------------------------------------------------
//  File:    UIToolKitHandler.cs  (Example / Commons)
//  Purpose: A small, reusable view layer that the UIToolKit example scenes
//           share. It owns references to the key UXML elements — the
//           dialogue container, speaker name label, body text, character
//           portrait, choice buttons — and exposes friendly methods the
//           rest of the examples can call without knowing UXML selectors.
// ============================================================================

using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

namespace KodeFlowStudios.Parley
{
	public class UIToolKitHandler : MonoBehaviour
	{
		struct RichChar
		{
			public char c;
			public string color;
		}

		// Drop the scene's UIDocument onto this field in the Inspector.
		[SerializeField] private UIDocument uiDocument;

		VisualElement dialogueContainer;
		Coroutine typewriteCoroutine;

		void Awake()
		{
			// The UXML template ships an element named "dialogueContainer" that
			// holds everything: speaker box, text box, portrait.
			dialogueContainer = uiDocument.rootVisualElement.Q<VisualElement>("dialogueContainer");
		}

		public void HideElements()
		{
			dialogueContainer.style.display = DisplayStyle.None;
		}

		public void ShowElements()
		{
			dialogueContainer.style.display = DisplayStyle.Flex;
		}

		public IEnumerator TypewriteRichText(Label label, string tagged, float charDelay = 0.05f)
		{
			var chars = new List<RichChar>();
			string currentColor = null;
			int i = 0;

			while (i < tagged.Length)
			{
				if (tagged[i] == '<')
				{
					int close = tagged.IndexOf('>', i);
					if (close != -1)
					{
						string tag = tagged.Substring(i, close - i + 1);
						if (tag.StartsWith("<color="))  currentColor = tag[7..^1];
						else if (tag == "</color>")     currentColor = null;
						i = close + 1;
						continue;
					}
				}
				chars.Add(new RichChar { c = tagged[i++], color = currentColor });
			}

			var sb = new System.Text.StringBuilder();
			for (int count = 0; count <= chars.Count; count++)
			{
				sb.Clear();
				for (i = 0; i < chars.Count; i++)
				{
					RichChar rc = chars[i];
					bool hidden = i >= count;

					if (hidden)
					{
						string hiddenColor = rc.color != null ? rc.color + "00" : "#00000000";
						sb.Append($"<color={hiddenColor}>{rc.c}</color>");
					}
					else
					{
						if (rc.color != null) sb.Append($"<color={rc.color}>");
						sb.Append(rc.c);
						if (rc.color != null) sb.Append("</color>");
					}
				}

				label.text = sb.ToString();
				if (count < chars.Count) yield return new WaitForSeconds(charDelay);
			}
		}

		/// <summary>
		/// Sets the speaker name box. Passing an empty string hides the box entirely
		/// </summary>
		public void SetSpeakerNameText(string text)
		{
			if (string.IsNullOrEmpty(text)) dialogueContainer.Q<VisualElement>("speakerNameBox").style.display = DisplayStyle.None;
			else
			{
				dialogueContainer.Q<VisualElement>("speakerNameBox").style.display = DisplayStyle.Flex;
				dialogueContainer.Q<VisualElement>("speakerNameBox").Q<Label>("speakerNameText").text = text;
			}
		}

		public void SetDialogueText(string text, bool typewrite=false)
		{
			if (typewrite) 
			{
				if (typewriteCoroutine != null) StopCoroutine(typewriteCoroutine);
				typewriteCoroutine = StartCoroutine(TypewriteRichText(dialogueContainer.Q<VisualElement>("dialogueBox").Q<Label>("dialogueText"), text));
			}
			else dialogueContainer.Q<VisualElement>("dialogueBox").Q<Label>("dialogueText").text = text;
		}

		/// <summary>Swaps the character pic. Pass <c>null</c> to clear it.</summary>
		public void SetImageSprite(Sprite sprite)
		{
			VisualElement imageElement = dialogueContainer.Q<VisualElement>("characterImage");
			imageElement.style.backgroundImage = new StyleBackground(sprite);
		}

		/// <summary>
		/// Flips text alignment for RTL languages (Arabic, Hebrew, etc).
		/// The UXML defaults to left-aligned, so flip to right for RTL and
		/// back for LTR.
		/// </summary>
		public void SetTextDirection(bool isRightToLeft)
		{
			var dialogueText = dialogueContainer.Q<VisualElement>("dialogueBox").Q<Label>("dialogueText");

			if (isRightToLeft)
			{
				dialogueText.style.unityTextAlign = TextAnchor.MiddleRight;
			}
			else
			{
				dialogueText.style.unityTextAlign = TextAnchor.MiddleLeft;
			}
		}

		/// <summary>
		/// Instantiates a choice button, wires up its click handler, and returns
		/// it so callers can tweak it further if they like. Buttons are tagged
		/// with the <c>dialogue-choice</c> USS class so <see cref="ClearChoiceButtons"/>
		/// can sweep them all away when the choice is made.
		/// </summary>
		public Button AddChoiceButton(int index, string buttonText, System.Action action)
		{
			var choiceButton = new Button()
			{
				text = buttonText,
				name = $"choiceButton{index}"
			};
			choiceButton.clicked += action;
			choiceButton.AddToClassList("dialogue-choice");

			dialogueContainer.Add(choiceButton);

			return choiceButton;
		}

		/// <summary>Removes every choice button that was added via <see cref="AddChoiceButton"/>.</summary>
		public void ClearChoiceButtons()
		{
			var choiceButtons = dialogueContainer.Query<Button>(className: "dialogue-choice").ToList();
			foreach (var button in choiceButtons)
			{
				button.RemoveFromHierarchy();
			}
		}
	}
}
