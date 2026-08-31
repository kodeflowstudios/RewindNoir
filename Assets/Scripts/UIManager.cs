using KodeFlowStudios.Parley.YamlCore;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	public enum Menu
	{
		Main,
		Settings
	};

	public static UIManager Instance { get; private set; }

	public ParleyYaml uiValues;
	public Menu currentMenu;

	public string GetUIText(Menu menu, string key)
	{
		if (currentMenu != menu || uiValues == null)
		{
			currentMenu = menu;
			uiValues = new ParleyYaml("Menus", menu.ToString());
		}
		return uiValues.GetNode(key).Text;
	}

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
		}
		else
		{
			Instance = this;
		}
	}
}
