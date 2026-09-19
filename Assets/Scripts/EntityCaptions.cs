using KodeFlowStudios.Parley.Localization;
using KodeFlowStudios.Parley.YamlCore;
using UnityEngine;
using UnityEngine.UIElements;

public class EntityCaptions : MonoBehaviour
{
    ParleyYaml entityCaptions;
	private Label captionLabel;

    void Start()
    {
		var _root = GetComponent<UIDocument>().rootVisualElement;
		captionLabel = _root.Q<Label>("label_captions");

		entityCaptions = new ParleyYaml("Misc", "EntityMonologue", Localizer.GetIDFromEnglishName(PlayerPrefs.GetString("Language")));
		entityCaptions.UnBindNextEvent();
		captionLabel.text = "";
	}

	public void ProgressDialogue()
	{
		captionLabel.text = entityCaptions.CurrentNode.Text;
		entityCaptions.ProgressDialogue();
	}
}
