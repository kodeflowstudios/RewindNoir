using KodeFlowStudios.Parley.YamlCore;
using UnityEngine;
using UnityEngine.UIElements;

public class UIMenu : MonoBehaviour
{
	public ParleyYaml uiValues;

	public string GetUIText(string menu, string name)
	{
		uiValues ??= new ParleyYaml("Menus", menu);
		return uiValues.GetNode(name).Text;
	}

    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();

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
}
