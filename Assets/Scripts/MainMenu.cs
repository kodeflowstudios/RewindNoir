using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
	public UIDocument settingsMenu;
	private VisualElement _root;

    void Start()
    {
		if (SettingsMenu.Instance.mainMenu == null) SettingsMenu.Instance.mainMenu = GetComponent<UIDocument>();

		settingsMenu.rootVisualElement.style.display = DisplayStyle.None;

		_root = GetComponent<UIDocument>().rootVisualElement;

		var playButton = _root.Q<Button>("button_play");
		playButton.clickable.clicked += () =>
		{
			SceneManager.LoadScene(1);
		};

		var settingsButton = _root.Q<Button>("button_settings");
		settingsButton.clickable.clicked += () =>
		{
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
    }
}
