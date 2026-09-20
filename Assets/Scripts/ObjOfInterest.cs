using System;
using System.Collections.Generic;
using KodeFlowStudios.Parley;
using KodeFlowStudios.Parley.Localization;
using KodeFlowStudios.Parley.YamlCore;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ObjOfInterest : MonoBehaviour
{
	public string folderName;
	public string fileName;
	public string nodeName;
	public RotateLookAt lookAt;
	public AudioSource dialogueSource;
	public List<AudioClip> dialogueClips;
	public PlayerController playerController;
	public UIToolKitHandler uiToolKitHandler;

	[Serializable]
	public class FinishedEvent : UnityEvent {}
	[SerializeField]
	private FinishedEvent m_OnFinish = new FinishedEvent();

	public FinishedEvent OnFinish
	{
		get { return m_OnFinish; }
		set { m_OnFinish = value; }
	}

	public ParleyYaml parleyYaml;
	private InputAction nextDialogueInput;

	async public void StartDialogue()
	{
		if (GameManager.Instance.inDialogue) return;
		GameManager.Instance.inDialogue = true;

		uiToolKitHandler.dialogueClips = dialogueClips;
		uiToolKitHandler.dialogueSource = dialogueSource;

		parleyYaml = new ParleyYaml(folderName, fileName, Localizer.GetIDFromEnglishName(PlayerPrefs.GetString("Language")));

		lookAt?.LookAtPlayer();
		playerController.DisableMoving();

		uiToolKitHandler.ShowElements();

		nextDialogueInput ??= new InputAction("NextDialogue", binding: "<Mouse>/leftButton");

		parleyYaml.BindNextEvent(nextDialogueInput);
		parleyYaml.ListeningForAdvance = false;

		parleyYaml.ProgressDialogue(nodeName);

		while (!parleyYaml.ConversationEnded)
		{
			uiToolKitHandler.SetSpeakerNameText(parleyYaml.CurrentNode?.Speaker);
			uiToolKitHandler.SetDialogueText(parleyYaml.CurrentNode?.Text, true);

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

		lookAt?.Reset();
		playerController.EnableMoving();
		uiToolKitHandler.HideElements();

		GameManager.Instance.inDialogue = false;

		m_OnFinish.Invoke();
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
