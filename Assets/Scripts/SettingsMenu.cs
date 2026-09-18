using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using KodeFlowStudios.Parley.Localization;

public class SettingsMenu : MonoBehaviour
{
	public UIDocument mainMenu;
	public bool isPaused;
	public bool notePadOpen;
	public InputActionReference pauseAction;
	private VisualElement _pauseMenu;
	private VisualElement _settingsMenu;
	private VisualElement _root;

	public static SettingsMenu Instance;

	void OnEnable()
	{
		pauseAction.action.performed += Pause;
	}

	void OnDisable() 
	{	
		pauseAction.action.performed -= Pause;
	}

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != this)
		{
			Destroy(this);
		}
		DontDestroyOnLoad(this);
	}

    void Start()
    {
		_root = GetComponent<UIDocument>().rootVisualElement;

		_pauseMenu = _root.Q<VisualElement>("Pause");
		_settingsMenu = _root.Q<VisualElement>("Settings");

		var resumeButton = _pauseMenu.Q<Button>("button_resume");
		resumeButton.clickable.clicked += () =>
		{
			isPaused = false;
			Time.timeScale = 1f;
			_root.style.display = DisplayStyle.None;
			GameManager.Instance?.GetPlayer()?.EnableMoving();
			GameManager.Instance?.GetPlayer()?.UpdateSensitivity();
		};

		var settingsButton = _pauseMenu.Q<Button>("button_settings");
		settingsButton.clickable.clicked += () =>
		{
			_pauseMenu.style.display = DisplayStyle.None;
			_settingsMenu.style.display = DisplayStyle.Flex;
		};

		var quitButton = _pauseMenu.Q<Button>("button_quit");
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

		var sensSlider = _settingsMenu.Q<Slider>("slider_sens");
		sensSlider.value = PlayerPrefs.GetFloat("Sensitivity", 1f);
		sensSlider.RegisterCallback<ChangeEvent<float>>((evt) =>
		{
			PlayerPrefs.SetFloat("Sensitivity", evt.newValue);
		});
		sensSlider.Q<Label>(className: "unity-base-field__label").name = "label_slider_sens";

		var langDropdown = _settingsMenu.Q<DropdownField>("dropdown_lang");
		langDropdown.RegisterValueChangedCallback(evt => PlayerPrefs.SetString("Language", evt.newValue));

		foreach (var (id, info) in Localizer.Languages)
		{
			langDropdown.choices.Add(info.EnglishName);
		}

		langDropdown.value = PlayerPrefs.GetString("Language");
		langDropdown.Q<Label>(className: "unity-base-field__label").name = "label_dropdown_lang";

		var backButton = _settingsMenu.Q<Button>("button_back");
		backButton.clickable.clicked += () =>
		{
			if (mainMenu != null)
			{
				_root.style.display = DisplayStyle.None;
				mainMenu.rootVisualElement.style.display = DisplayStyle.Flex;
			}
			else
			{
				_pauseMenu.style.display = DisplayStyle.Flex;
				_settingsMenu.style.display = DisplayStyle.None;
			}
		};
    }

    private void Pause(InputAction.CallbackContext context)
    {
		if (notePadOpen) return;
		if (mainMenu != null) return;
		if (GameManager.Instance.inDialogue) return;
		
		if (_root.style.display == DisplayStyle.None)
		{
			isPaused = true;
			_root.style.display = DisplayStyle.Flex;
			_pauseMenu.style.display = DisplayStyle.Flex;
			_settingsMenu.style.display = DisplayStyle.None;
			Time.timeScale = 0f;
			GameManager.Instance?.GetPlayer()?.DisableMoving();
		}
		else
		{
			isPaused = false;
			Time.timeScale = 1f;
			_root.style.display = DisplayStyle.None;
			GameManager.Instance?.GetPlayer()?.EnableMoving();
			GameManager.Instance?.GetPlayer()?.UpdateSensitivity();
		}
    }
}
