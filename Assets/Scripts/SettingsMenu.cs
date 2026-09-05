using UnityEngine;
using UnityEngine.UIElements;

public class SettingsMenu : MonoBehaviour
{
	public UIDocument mainMenu;
	private UIDocument _settingsMenu;

    void Start()
    {
		_settingsMenu = GetComponent<UIDocument>();
		var root = _settingsMenu.rootVisualElement;

		var sensSlider = root.Q<Slider>("slider_sens");
		sensSlider.RegisterCallback<ChangeEvent<float>>((evt) =>
		{
			PlayerPrefs.SetFloat("Sensitivity", evt.newValue);
		});

		var langDropdown = root.Q<DropdownField>("dropdown_lang");
		langDropdown.RegisterValueChangedCallback(evt => PlayerPrefs.SetString("Language", evt.newValue));
		langDropdown.Add(new Label("English"));

		var backButton = root.Q<Button>("button_back");
		backButton.clickable.clicked += () =>
		{
			mainMenu.enabled = true;
			_settingsMenu.enabled = false;
		};

		_settingsMenu.enabled = false;
    }
}
