using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
	public UIDocument settingsMenu;
	private UIDocument _mainMenu;

    void Start()
    {
		_mainMenu = GetComponent<UIDocument>();
		var root = _mainMenu.rootVisualElement;

		var playButton = root.Q<Button>("button_play");
		playButton.clickable.clicked += () =>
		{
			SceneManager.LoadScene(0);
		};

		var settingsButton = root.Q<Button>("button_settings");
		settingsButton.clickable.clicked += () =>
		{
			settingsMenu.enabled = true;
			_mainMenu.enabled = false;
		};

		var quitButton = root.Q<Button>("button_quit");
		quitButton.clickable.clicked += () =>
		{
			Application.Quit();
		};
    }
}
