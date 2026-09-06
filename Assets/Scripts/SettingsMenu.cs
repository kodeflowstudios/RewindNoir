using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SettingsMenu : MonoBehaviour
{
	public UIDocument mainMenu;
	private VisualElement _root;
	public InputActionReference pauseAction;

	public static SettingsMenu Instance;

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

	void OnEnable()
	{
		pauseAction.action.performed += Pause;
	}

	void OnDisable() 
	{	
		pauseAction.action.performed -= Pause;
	}

    private void Pause(InputAction.CallbackContext context)
    {
		if (mainMenu != null) return;
		
		if (_root.style.display == DisplayStyle.None)
		{
			_root.style.display = DisplayStyle.Flex;
			GameManager.Instance?.GetPlayer()?.DisableMoving();
		}
		else
		{
			_root.style.display = DisplayStyle.None;
			GameManager.Instance?.GetPlayer()?.EnableMoving();
			GameManager.Instance?.GetPlayer()?.UpdateSensitivity();
		}
    }

    void Start()
    {
		_root = GetComponent<UIDocument>().rootVisualElement;

		var sensSlider = _root.Q<VisualElement>("settings_container").Q<Slider>("slider_sens");
		sensSlider.value = PlayerPrefs.GetFloat("Sensitivity", 1f);
		Debug.Log(sensSlider.value);
		sensSlider.RegisterCallback<ChangeEvent<float>>((evt) =>
		{
			PlayerPrefs.SetFloat("Sensitivity", evt.newValue);
		});

		var langDropdown = _root.Q<DropdownField>("dropdown_lang");
		langDropdown.RegisterValueChangedCallback(evt => PlayerPrefs.SetString("Language", evt.newValue));
		langDropdown.choices.Add("English");
		langDropdown.value = "English";

		var backButton = _root.Q<Button>("button_back");
		backButton.clickable.clicked += () =>
		{
			_root.style.display = DisplayStyle.None;
			GameManager.Instance?.GetPlayer()?.EnableMoving();
			GameManager.Instance?.GetPlayer()?.UpdateSensitivity();
		};
    }
}
