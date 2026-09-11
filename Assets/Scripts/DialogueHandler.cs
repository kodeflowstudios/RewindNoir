using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using KodeFlowStudios.Parley;
using KodeFlowStudios.Parley.YamlCore;

public class DialogueHandler : MonoBehaviour
{
	public NPC npc;
	public PlayerController playerController;
	public UIDocument resultScreen;
	public UIToolKitHandler uiToolKitHandler;
	ParleyYaml parleyYaml;

	void Start()
	{
		uiToolKitHandler.HideElements();
		resultScreen.rootVisualElement.style.display = DisplayStyle.None;
		parleyYaml = GameManager.Instance?.npcDialogue;
	}

	async public void StartDialogue()
	{
		GameManager.Instance.inDialogue = true;

		npc.LookAtPlayer();
		playerController.DisableMoving();

		GameManager.Instance.obu.HideObjective();

		uiToolKitHandler.ShowElements();

		if (GameManager.Instance.hasTalked)
		{
			parleyYaml.ConversationEnded = false;
			parleyYaml.BindNextEvent(new InputAction("NextDialogue", binding: "<Mouse>/leftButton"));
			parleyYaml.ProgressDialogue("node_10");
		}

		while (parleyYaml.ConversationEnded != true)
		{
			uiToolKitHandler.SetSpeakerNameText(parleyYaml.CurrentNode.Speaker);
			uiToolKitHandler.SetDialogueText(parleyYaml.CurrentNode.Text);

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

				await parleyYaml.GetPlayerChoice();
			}
			else await parleyYaml.OnNextDialogue;
		}

		if (!GameManager.Instance.hasTalked)
		{
			GameManager.Instance.obu.UpdateObjective();
			GameManager.Instance.hasTalked = true;
		}

		playerController.EnableMoving();
		uiToolKitHandler.HideElements();

		if (parleyYaml.Flags.IsFlagSet("has_won"))
		{
			resultScreen.rootVisualElement.style.display = DisplayStyle.Flex;
			resultScreen.GetComponent<ResultScreen>().ShowResult("You Win!", Color.green);
			playerController.DisableMoving();
		}
		else if (parleyYaml.Flags.IsFlagSet("has_lost"))
		{
			resultScreen.rootVisualElement.style.display = DisplayStyle.Flex;
			resultScreen.GetComponent<ResultScreen>().ShowResult("You Lose...", Color.red);
			playerController.DisableMoving();
		}

		GameManager.Instance.inDialogue = false;
		GameManager.Instance.canShowNotepad = true;

		GameManager.Instance.obu.ShowObjective();
	}
}
