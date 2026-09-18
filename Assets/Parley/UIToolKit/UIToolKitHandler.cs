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
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

namespace KodeFlowStudios.Parley
{
	public class UIToolKitHandler : MonoBehaviour
	{
		[SerializeField] private UIDocument uiDocument;

		public List<AudioClip> dialogueClips;
		public AudioSource dialogueSource;

		[SerializeField] private float typewriteCharactersPerSecond = 45f;
		[SerializeField] private float punctuationPause = 0.16f;

		[SerializeField] private float inputLockoutDuration = 0.15f;

		VisualElement root;
		VisualElement dialogueContainer;
		Coroutine typingCoroutine;

		public bool IsTyping { get; private set; }

		private string currentTypewriteText = string.Empty;
		private Label currentDialogueLabel;

		private float inputLockedUntil = -1f;

		void Awake()
		{
			root = uiDocument.rootVisualElement;
			dialogueContainer = root.Q<VisualElement>("dialogueContainer");
			dialogueContainer.Q<VisualElement>("dialogueBox").AddToClassList(
			GameManager.Instance?.currentScene switch
			{
				GameManager.Scenes.APARTMENT_VOID
				| GameManager.Scenes.CITY_VOID => "void",
				_ => "normal"
			});
		}

		void Update()
		{
			if (!IsTyping)
				return;

			if (IsInputLocked())
				return;

			if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
				SkipTypewrite();
		}

		private void OnEnable()
		{
			uiDocument?.rootVisualElement.RegisterCallback<PointerDownEvent>(HandlePointerDown, TrickleDown.TrickleDown);
		}

		private void OnDisable()
		{
			uiDocument?.rootVisualElement.UnregisterCallback<PointerDownEvent>(HandlePointerDown, TrickleDown.TrickleDown);
		}

		private bool IsInputLocked() => Time.unscaledTime < inputLockedUntil;

		private void LockInput()
		{
			inputLockedUntil = Time.unscaledTime + inputLockoutDuration;
		}

		private void HandlePointerDown(PointerDownEvent evt)
		{
			if (!IsTyping || evt.button != 0)
				return;

			if (evt.target is Button)
				return;

			if (IsInputLocked())
				return;

			SkipTypewrite();
		}

		public void HideElements()
		{
			root.style.display = DisplayStyle.None;
		}

		public void ShowElements()
		{
			root.style.display = DisplayStyle.Flex;
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

		public void SetDialogueText(string text, bool typewrite)
		{
			if (typingCoroutine != null)
			{
				StopCoroutine(typingCoroutine);
				typingCoroutine = null;
			}

			VisualElement dialogueBox = dialogueContainer?.Q<VisualElement>("dialogueBox");
			currentDialogueLabel = dialogueBox?.Q<Label>("dialogueText");
			currentTypewriteText = text ?? string.Empty;

			if (currentDialogueLabel == null)
			{
				IsTyping = false;
				Debug.LogError("[Parley] dialogueText Label was not found.");
				return;
			}

			if (!typewrite || currentTypewriteText.Length == 0)
			{
				IsTyping = false;
				currentDialogueLabel.text = currentTypewriteText;
				return;
			}

			LockInput();
			typingCoroutine = StartCoroutine(TypewriteText(currentTypewriteText, currentDialogueLabel));
		}

		public void SkipTypewrite()
		{
			if (!IsTyping)
				return;

			if (typingCoroutine != null)
			{
				StopCoroutine(typingCoroutine);
				typingCoroutine = null;
			}

			if (currentDialogueLabel != null)
				currentDialogueLabel.text = currentTypewriteText;

			IsTyping = false;

			dialogueSource.Stop();

			LockInput();
		}

		private IEnumerator TypewriteText(string text, Label dialogueText)
		{
			if (text == null || dialogueText == null)
			{
				IsTyping = false;
				typingCoroutine = null;
				yield break;
			}

			IsTyping = true;
			dialogueText.text = string.Empty;

			float characterDelay = 1f / Mathf.Max(1f, typewriteCharactersPerSecond);
			var visibleText = new System.Text.StringBuilder(text.Length);

			for (int i = 0; i < text.Length; i++)
			{
				char character = text[i];
				visibleText.Append(character);
				dialogueText.text = visibleText.ToString();

				float delay = characterDelay;

				if (character == '.' || character == '!' || character == '?')
					delay += punctuationPause;
				else if (character == ',' || character == ';' || character == ':')
					delay += punctuationPause * 0.5f;

				if (!dialogueSource.isPlaying)
				{
					dialogueSource.clip = dialogueClips[Random.Range(0, dialogueClips.Count)];
					dialogueSource.Play();
				}

				yield return new WaitForSecondsRealtime(delay);
			}

			dialogueText.text = text;
			IsTyping = false;
			typingCoroutine = null;
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
			// Choices just appeared — protect against a click that's still
			// "in flight" from whatever just happened (a skip, an advance)
			// landing on a button it was never actually aimed at.
			LockInput();

			var choiceButton = new Button()
			{
				text = buttonText,
				name = $"choiceButton{index}"
			};

			choiceButton.clicked += () =>
			{
				if (IsInputLocked())
					return;

				action();
			};
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
