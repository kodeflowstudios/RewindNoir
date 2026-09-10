using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Notepad : MonoBehaviour
{
	public bool canShow = true;
	public InputActionReference toggleAction;
	public AudioClip noteBookUpSFX;
	public AudioClip noteBookDownSFX;
	public List<AudioClip> pageTurnSFX;
	public AnimationCurve animationCurve;
	private VisualElement _root;
	private VisualElement _pageOne;
	private VisualElement _pageTwo;
	private Label _pageDisplay;
	private Button _prevButton;
	private Button _nextButton;
	private AudioSource _audioSource;
	private float _slideDuration = 0.3f;
	private int _currentPage = 1;
	private int _totalPages = 2;
	private bool _isShown = false;

    void Start()
    {
		_root = GetComponent<UIDocument>().rootVisualElement;

		_pageOne = _root.Q<VisualElement>("page_one");
		_pageTwo = _root.Q<VisualElement>("page_two");

		_prevButton = _root.Q<VisualElement>("footer").Q<Button>("button_prev");
		_prevButton.clickable.clicked += () =>
		{
			_currentPage = Mathf.Max(1, _currentPage - 1);
			RefreshPage();
		};

		_pageDisplay = _root.Q<VisualElement>("footer").Q<Label>("label_page");

		_nextButton = _root.Q<VisualElement>("footer").Q<Button>("button_next");
		_nextButton.clickable.clicked += () =>
		{
			_currentPage = Mathf.Max(_currentPage + 1, _totalPages);
			RefreshPage();
		};

		_root.style.display = DisplayStyle.None;
		_audioSource = gameObject.AddComponent<AudioSource>();
    }

    void OnEnable() => toggleAction.action.performed += Toggle;
    void OnDisable() => toggleAction.action.performed -= Toggle;

    private void Toggle(InputAction.CallbackContext context)
    {
		if (!canShow) return;
		if (!_isShown) StartSlideUp();
		else StartSlideDown();
	}

    void StartSlideUp()
	{
		StartCoroutine(SlideUp());
	}

	IEnumerator SlideUp()
	{
		_root.style.display = DisplayStyle.Flex;

		float timeElapsed = 0;

		while (timeElapsed < _slideDuration)
		{
			float t = timeElapsed/_slideDuration;

			t = animationCurve.Evaluate(t);
			_root.style.marginTop = new StyleLength(new Length(Mathf.Lerp(100, 0, t), LengthUnit.Percent));

			timeElapsed += Time.deltaTime;

			yield return null;
		}

		_root.style.marginTop = 0;

		_isShown = true;
		_audioSource.clip = noteBookUpSFX;
		_audioSource.Play();
	}

	void StartSlideDown()
	{
		StartCoroutine(SlideDown());
	}

	IEnumerator SlideDown()
	{
		float timeElapsed = 0;

		while (timeElapsed < _slideDuration)
		{
			float t = timeElapsed/_slideDuration;

			t = animationCurve.Evaluate(1-t);

			_root.style.marginTop = new StyleLength(new Length(Mathf.Lerp(100, 0, t), LengthUnit.Percent));

			timeElapsed += Time.deltaTime;

			yield return null;
		}

		_root.style.marginTop = 0;

		_root.style.display = DisplayStyle.None;

		_isShown = false;
		_audioSource.clip = noteBookDownSFX;
		_audioSource.Play();
	}

	void RefreshPage()
	{
		_pageOne.style.display = _currentPage switch 
		{
			1 => DisplayStyle.Flex,
			_ => DisplayStyle.None,
		};

		_pageTwo.style.display = _currentPage switch 
		{
			2 => DisplayStyle.Flex,
			_ => DisplayStyle.None,
		};

		_pageDisplay.text = $"({_currentPage}/{_totalPages})";

		_prevButton.SetEnabled(_currentPage > 1);
		_nextButton.SetEnabled(_currentPage < _totalPages);

		_audioSource.clip = pageTurnSFX[Random.Range(0, pageTurnSFX.Count)];
		_audioSource.Play();
	}
}
