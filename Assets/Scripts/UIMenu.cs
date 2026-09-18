using UnityEngine;
using UnityEngine.UIElements;
using KodeFlowStudios.Parley.YamlCore;
using KodeFlowStudios.Parley.Localization;

public class UIMenu : MonoBehaviour
{
	public ParleyYaml uiValues;

	public LanguageDirection GetUIDirection(string menu)
	{
		uiValues ??= new ParleyYaml("Menus", menu, Localizer.GetIDFromEnglishName(PlayerPrefs.GetString("Language")));
		return uiValues.Meta.TextDirection == TextDirection.LTR ? LanguageDirection.LTR : LanguageDirection.RTL;
	}

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
				label.languageDirection = GetUIDirection(menu);

				var languageClass = GetUIDirection(menu) == LanguageDirection.LTR ? "english" : "arabic";
				label.AddToClassList(languageClass);
			});

			root.Query<Button>().ForEach(button =>
			{
				button.text = GetUIText(menu, button.name);
				button.languageDirection = GetUIDirection(menu);

				var languageClass = GetUIDirection(menu) == LanguageDirection.LTR ? "english" : "arabic";
				button.AddToClassList(languageClass);
			});
		}
    }
}
