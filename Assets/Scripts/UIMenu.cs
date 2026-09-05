using KodeFlowStudios.Parley.Localization;
using KodeFlowStudios.Parley.YamlCore;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class UIMenu : MonoBehaviour
{
	public ParleyYaml uiValues;

	public string GetUIText(string menu, string name)
	{
		uiValues ??= new ParleyYaml("Menus", menu, Localizer.GetIDFromEnglishName(PlayerPrefs.GetString("Language")));
		return uiValues.GetNode(name).Text;
	}

    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
		if (uiDocument != null)
		{
			VisualElement root = uiDocument.rootVisualElement;
			string menu = root.Q(className: "main-container").name;

			root.Query<Label>().ForEach(label =>
			{
				label.text = GetUIText(menu, label.name);
			});

			root.Query<Button>().ForEach(button =>
			{
				button.text = GetUIText(menu, button.name);
			});
		}
		else
		{
			var tmp_texts = GetComponents<TMP_Text>();
			foreach (TMP_Text t in tmp_texts)
			{
				t.text = GetUIText(t.gameObject.tag, t.text);
			}

			var child_texts = GetComponentsInChildren<TMP_Text>();
			foreach (TMP_Text t in child_texts)
			{
				t.text = GetUIText(t.gameObject.tag, t.text);
			}
		}
    }
}
