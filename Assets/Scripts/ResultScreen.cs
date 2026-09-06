using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ResultScreen : MonoBehaviour
{
	private VisualElement _root;
	private Label resultLabel;

    void Start()
    {
		_root = GetComponent<UIDocument>().rootVisualElement;

		_root.style.display = DisplayStyle.None;

		resultLabel = _root.Q<Label>("label_result");

		var menuButton = _root.Q<Button>("button_main_menu");
		menuButton.clickable.clicked += () =>
		{
            SceneManager.LoadScene(0);
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

	public void ShowResult(string textResult, Color bgColor)
	{
		resultLabel.text = textResult;
		_root.style.display = DisplayStyle.Flex;
		_root.style.backgroundImage = null;
		var bgColorTrans = bgColor;
		bgColorTrans.a = 0.4f;
		_root.style.backgroundColor = bgColorTrans;
	}
}
