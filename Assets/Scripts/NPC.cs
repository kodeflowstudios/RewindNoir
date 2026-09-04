using UnityEngine;
using UnityEngine.Animations;
using KodeFlowStudios.Parley.YamlCore;
using KodeFlowStudios.Parley;

public class NPC : MonoBehaviour
{
	public Transform player;
	public ObjectiveUpdater obu;
	public UIToolKitHandler uiToolKitHandler;

	void Start()
	{
		uiToolKitHandler.HideElements();
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
		var pc = player.GetComponent<PlayerController>();
		pc.DisableMoving();
		ParleyYaml parleyYaml = new ParleyYaml("Dialogues", "Cop");

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

		pc.EnableMoving();
		uiToolKitHandler.HideElements();
		parleyYaml = null;
		obu.UpdateObjectives();
	}
}
