using UnityEngine;
using UnityEngine.UIElements;
using KodeFlowStudios.Parley;
using KodeFlowStudios.Parley.YamlCore;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DialogueHandler : MonoBehaviour
{
	public string folderName;
	public string fileName;
	public string continueNode = "node_10";
	public RotateLookAt lookAt;
	public PlayerController playerController;
	public AudioSource dialogueSource;
	public List<AudioClip> dialogueClips;
	public UIDocument resultScreen;
	public UIToolKitHandler uiToolKitHandler;
	ParleyYaml parleyYaml;

	private InputAction nextDialogueInput;

	void Start()
	{
		uiToolKitHandler.HideElements();
		resultScreen.rootVisualElement.style.display = DisplayStyle.None;
		GameManager.Instance?.LoadDialogue(folderName, fileName);
		parleyYaml = GameManager.Instance?.npcDialogue;
	}

	async public void StartDialogue()
	{
		if (GameManager.Instance.inDialogue) return;
		GameManager.Instance.inDialogue = true;

		uiToolKitHandler.dialogueClips = dialogueClips;
		uiToolKitHandler.dialogueSource = dialogueSource;

		lookAt?.LookAtPlayer();
		playerController.DisableMoving();

		GameManager.Instance.obu.HideObjective();

		uiToolKitHandler.ShowElements();

		nextDialogueInput ??= new InputAction("NextDialogue", binding: "<Mouse>/leftButton");

		parleyYaml.BindNextEvent(nextDialogueInput);
		parleyYaml.ListeningForAdvance = false;

		if (GameManager.Instance.hasTalked)
		{
			parleyYaml.ConversationEnded = false;
			parleyYaml.ProgressDialogue(continueNode);
		}

		while (!parleyYaml.ConversationEnded)
		{
			uiToolKitHandler.SetSpeakerNameText(parleyYaml.CurrentNode.Speaker);
			uiToolKitHandler.SetDialogueText(parleyYaml.CurrentNode.Text, true);

			while (uiToolKitHandler.IsTyping)
				await System.Threading.Tasks.Task.Yield();

			await System.Threading.Tasks.Task.Yield();

			var choices = parleyYaml.GetCurrentChoices();
			if (choices.Count > 0)
			{
				for (int x = 0; x < parleyYaml.CurrentNode.Choices.Count; x++)
				{
					int choiceIndex = x;
					uiToolKitHandler.AddChoiceButton(x, parleyYaml.CurrentNode.Choices[x].Text, () =>
					{
						parleyYaml.ChoiceMade(choiceIndex);
						uiToolKitHandler.ClearChoiceButtons();
					});
				}

				parleyYaml.ListeningForAdvance = false;
				await parleyYaml.GetPlayerChoice();
			}
			else
			{
				parleyYaml.ListeningForAdvance = true;

				await parleyYaml.OnNextDialogue;

				parleyYaml.ListeningForAdvance = false;
			}
		}

		if (!GameManager.Instance.hasTalked)
		{
			GameManager.Instance.obu.UpdateObjective();
			GameManager.Instance.hasTalked = true;
		}

		playerController.EnableMoving();
		uiToolKitHandler.HideElements();

		GameManager.Instance.inDialogue = false;

		GameManager.Instance.obu.ShowObjective();

		if (parleyYaml.Flags.IsFlagSet("selected") && GameManager.Instance.currentScene == GameManager.Scenes.CITY)
		{
			GameManager.Instance.hasTalked = false;
			GameManager.Instance.inDialogue = false;
			GameManager.Instance.enteredEntropy = false;
			SceneSwitcher.SwitchScene("Apartment");
		}
	}

	private void OnDestroy()
	{
		if (nextDialogueInput != null)
		{
			nextDialogueInput.Disable();
			nextDialogueInput.Dispose();
			nextDialogueInput = null;
		}
	}
}
