using UnityEngine;
using UnityEngine.UIElements;

public class EndScreen : MonoBehaviour
{
	private VisualElement _root;

    void Start()
    {
		_root = GetComponent<UIDocument>().rootVisualElement;

		var menuButton = _root.Q<Button>("button_main_menu");
		menuButton.clickable.clicked += () =>
		{
			Time.timeScale = 1f;
			SceneSwitcher.SwitchScene("MainMenu");
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
