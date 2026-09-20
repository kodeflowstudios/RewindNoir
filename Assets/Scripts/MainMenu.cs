using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
	public UIDocument settingsMenu;
	public UIDocument tutorialMenu;
	private VisualElement _root;

    void Start()
    {
		if (GameManager.Instance != null) Destroy(GameManager.Instance);
		if (SettingsMenu.Instance.mainMenu == null) SettingsMenu.Instance.mainMenu = GetComponent<UIDocument>();

		settingsMenu.rootVisualElement.style.display = DisplayStyle.None;

		_root = GetComponent<UIDocument>().rootVisualElement;

		var playButton = _root.Q<Button>("button_play");
		playButton.clickable.clicked += () =>
		{
			_root.style.display = DisplayStyle.None;
			tutorialMenu.rootVisualElement.style.display = DisplayStyle.Flex;
		};

		var settingsButton = _root.Q<Button>("button_settings");
		settingsButton.clickable.clicked += () =>
		{
			_root.style.display = DisplayStyle.None;
			settingsMenu.rootVisualElement.style.display = DisplayStyle.Flex;
		};

		var quitButton = _root.Q<Button>("button_quit");
		quitButton.clickable.clicked += () =>
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
			Application.OpenURL(webplayerQuitURL);
#else
			Application.Quit();
#endif
		};

		UnityEngine.Cursor.visible = true;
		UnityEngine.Cursor.lockState = CursorLockMode.Confined;
    }
}
