using TMPro;
using UnityEngine;
using KodeFlowStudios.Parley.YamlCore;
using KodeFlowStudios.Parley.Localization;

public class ObjectiveUpdater : MonoBehaviour
{
	public TMP_Text ltrObjectiveText;
	public TMP_Text rtlObjectiveText;

	ParleyYaml parleyYaml;

	void Start()
	{
		GameManager.Instance.obu = this;
		parleyYaml = GameManager.Instance.objectivesDialogue;
		parleyYaml.UnBindNextEvent();
		UpdateObjective();
	}

	public void UpdateObjective(string objective_name="")
	{
		if (!string.IsNullOrEmpty(objective_name)) parleyYaml.ProgressDialogue(objective_name); 

		if (parleyYaml.Meta.TextDirection == TextDirection.LTR)
		{
			ltrObjectiveText.text = parleyYaml.CurrentNode.Text;
			rtlObjectiveText.text = "";
		}
		else
		{
			ltrObjectiveText.text = "";
			rtlObjectiveText.text = parleyYaml.CurrentNode.Text;
		}

		parleyYaml.ProgressDialogue();

		if (parleyYaml.ConversationEnded) parleyYaml = null;
	}

	public void HideObjective()
	{
		ltrObjectiveText.enabled = false;
		rtlObjectiveText.enabled = false;
	}

	public void ShowObjective()
	{
		ltrObjectiveText.enabled = true;
		rtlObjectiveText.enabled = true;
	}
}
