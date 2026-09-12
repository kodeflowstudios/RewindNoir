using UnityEngine;
using KodeFlowStudios.Parley.YamlCore;
using KodeFlowStudios.Parley.Localization;
using UnityEngine.UIElements;

public class IntroCutscene : MonoBehaviour
{
    ParleyYaml cutsceneCaptions;
	private Label captionLabel;

    void Start()
    {
		var _root = GetComponent<UIDocument>().rootVisualElement;
		captionLabel = _root.Q<Label>("label_captions");

		cutsceneCaptions = new ParleyYaml("Misc", "Cutscene", Localizer.GetIDFromEnglishName(PlayerPrefs.GetString("Language")));
		cutsceneCaptions.UnBindNextEvent();
		captionLabel.text = "";
	}

	public void ProgressDialogue()
	{
		captionLabel.text = cutsceneCaptions.CurrentNode.Text;
		cutsceneCaptions.ProgressDialogue();
	}
}
