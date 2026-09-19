using UnityEngine;
using UnityEngine.UIElements;

public class TutorialScreen : MonoBehaviour
{
	private VisualElement _root;

	void Start()
	{
		if (GameManager.Instance != null) Destroy(GameManager.Instance);
		if (SettingsMenu.Instance.mainMenu == null) SettingsMenu.Instance.mainMenu = GetComponent<UIDocument>();

		_root = GetComponent<UIDocument>().rootVisualElement;

		var playButton = _root.Q<Button>("button_continue");
		playButton.clickable.clicked += () =>
		{
			SceneSwitcher.SwitchScene("Intro");
		};
	}
}
