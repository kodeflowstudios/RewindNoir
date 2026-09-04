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
		// parleyYaml = new ParleyYaml("Misc", "Objectives", Localizer.GetIDFromEnglishName(PlayerPrefs.GetString("lang")));
		parleyYaml = new ParleyYaml("Misc", "Objectives");
		parleyYaml.UnBindNextEvent();
		UpdateObjectives();
	}

	public void UpdateObjectives()
	{
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
}
