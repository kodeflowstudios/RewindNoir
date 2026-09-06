using UnityEngine;
using KodeFlowStudios.Parley;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using KodeFlowStudios.Parley.YamlCore;

public class NPC : MonoBehaviour
{
	public Transform player;
	PlayerController pc;
	public ObjectiveUpdater obu;
	public UIToolKitHandler uiToolKitHandler;
	public UIDocument resultScreen;
	ParleyYaml parleyYaml;

	void Start()
	{
		uiToolKitHandler.HideElements();
		resultScreen.rootVisualElement.style.display = DisplayStyle.None;
		parleyYaml = GameManager.Instance?.parleyYaml;
		pc = player.GetComponent<PlayerController>();
	}

	void LookAtPlayer()
	{
		var lookPos = player.position - transform.position;
		lookPos.y = 0;
		transform.rotation = Quaternion.LookRotation(lookPos);
	}

	async public void StartDialogue()
	{
		LookAtPlayer();
		pc.DisableMoving();

		if (GameManager.Instance.hasTalked)
		{
			parleyYaml.ConversationEnded = false;
			parleyYaml.BindNextEvent(new InputAction("NextDialogue", binding: "<Mouse>/leftButton"));
			parleyYaml.ProgressDialogue("node_10");
		}

		uiToolKitHandler.ShowElements();

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
			obu.UpdateObjectives();
			GameManager.Instance.hasTalked = true;
		}

		pc.EnableMoving();
		uiToolKitHandler.HideElements();

		if (parleyYaml.Flags.IsFlagSet("has_won"))
		{
			resultScreen.rootVisualElement.style.display = DisplayStyle.Flex;
			resultScreen.GetComponent<ResultScreen>().ShowResult("You Win!", Color.green);
			pc.DisableMoving();
		}
		else if (parleyYaml.Flags.IsFlagSet("has_lost"))
		{
			resultScreen.rootVisualElement.style.display = DisplayStyle.Flex;
			resultScreen.GetComponent<ResultScreen>().ShowResult("You Lose...", Color.red);
			pc.DisableMoving();
		}
	}
}
