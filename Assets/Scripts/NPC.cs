using UnityEngine;
using KodeFlowStudios.Parley.YamlCore;
using KodeFlowStudios.Parley;
using UnityEngine.InputSystem;

public class NPC : MonoBehaviour
{
	public Transform player;
	public ObjectiveUpdater obu;
	public UIToolKitHandler uiToolKitHandler;
	ParleyYaml parleyYaml;
	bool _hasTalked = false;

	void Start()
	{
		uiToolKitHandler.HideElements();
		parleyYaml = new ParleyYaml("Dialogues", "Cop");
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

		if (_hasTalked)
		{
			parleyYaml.ConversationEnded = false;
			parleyYaml.BindNextEvent(new InputAction("NextDialogue", binding: "<Mouse>/leftButton"));
			parleyYaml.ProgressDialogue("node_3");
		}

		uiToolKitHandler.ShowElements();

		while (parleyYaml.ConversationEnded != true)
		{
			uiToolKitHandler.SetSpeakerNameText(parleyYaml.CurrentNode.Speaker);
			uiToolKitHandler.SetDialogueText(parleyYaml.CurrentNode.Text, true);

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

		if (!_hasTalked)
		{
			obu.UpdateObjectives();
			_hasTalked = true;
		}

		pc.EnableMoving();
		uiToolKitHandler.HideElements();
	}
}
