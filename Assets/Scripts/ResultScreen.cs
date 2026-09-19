using UnityEngine;
using UnityEngine.UIElements;

public class ResultScreen : MonoBehaviour
{
	private VisualElement _root;
	private Label resultLabel;

    void Start()
    {
		_root = GetComponent<UIDocument>().rootVisualElement;

		resultLabel = _root.Q<Label>("label_result");

		var retryButton = _root.Q<Button>("button_try_again");
		retryButton.clickable.clicked += () =>
		{
			Time.timeScale = 1f;
			SceneSwitcher.ReloadScene();
		};

		var menuButton = _root.Q<Button>("button_main_menu");
		menuButton.clickable.clicked += () =>
		{
			Time.timeScale = 1f;
			SceneSwitcher.SwitchScene("MainMenu");
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
		Time.timeScale = 0f;
	}
}
