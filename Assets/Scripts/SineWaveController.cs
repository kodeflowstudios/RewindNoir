using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SineWaveController : MonoBehaviour
{
	public float currentValue;
	public float targetValue;
	public float sensitivity = 0.3f;
	public float amplitudeMultiplier = 50f;
	public float peakAmplitude = 120f;
	public float frequencyMultiplier = 0.5f;
	public float peakFrequency = 6f;
	public InputActionReference mouseScrollYAction;
	public AudioClip angelicChoirSFX;
	public AudioClip correctChimeSFX;
	public AudioClip successChimeSFX;
	private AudioSource _audioSource;
	private VisualElement _root;
	private Label _tutorialLabel;
	private SineWaveElement _mainSineWave;
	private List<SineWaveElement> _sineWaves = new();
	private Coroutine _checkAccuracyCoroutine;
	private bool _confirmed = false;
	private float _opacity = 1f;

    void Start()
    {
		_audioSource = GetComponent<AudioSource>();

		_root = GetComponent<UIDocument>().rootVisualElement;

		_tutorialLabel = _root.Q<Label>("label_tutorial");

		_root.style.display = DisplayStyle.Flex;

		_root.Query<SineWaveElement>().ForEach(element =>
		{
			if (element.ClassListContains("target")) _mainSineWave = element;
			else _sineWaves.Add(element);
		});

		targetValue = Random.Range(4f, 12f);

		_mainSineWave.amplitude = Mathf.Min(targetValue*amplitudeMultiplier, peakAmplitude);
		_mainSineWave.frequency = Mathf.Min(targetValue*frequencyMultiplier, peakFrequency);

		for (int i = 0; i < _sineWaves.Count; i++)
		{
			float randValue = Random.Range(1f, 12f);

			_sineWaves[i].amplitude = Mathf.Min(randValue*amplitudeMultiplier*1.2f, peakAmplitude*1.7f);
			_sineWaves[i].frequency = Mathf.Min(randValue*frequencyMultiplier*1.4f, peakFrequency*1.5f);
		}

		currentValue = Random.Range(0f, 12f);
		float diff = Mathf.Abs(currentValue - targetValue);

		while (diff < 3f)
		{
			currentValue = Random.Range(0f, 12f);
			diff = Mathf.Abs(currentValue - targetValue);
		}

		_audioSource.volume = Mathf.Max(0f, 0.8f - (diff/4f));

		GameManager.Instance?.GetPlayer()?.DisableMoving();
    }

	void OnEnable()
	{
		mouseScrollYAction.action.performed += UpdateScrollValue;
	}

    void OnDisable()
	{
		mouseScrollYAction.action.performed -= UpdateScrollValue;
	}

    private void UpdateScrollValue(InputAction.CallbackContext context)
    {
		currentValue += context.ReadValue<float>() * sensitivity;
		currentValue = Mathf.Clamp(currentValue, 0, 12f);
    }

	void Update()
	{
		for (int i = 0; i < _sineWaves.Count; i++)
		{
			float currentAmplitude = _sineWaves[i].amplitude;
			float currentFrequency = _sineWaves[i].frequency;
			float currentPhase = _sineWaves[i].phase;

			float targetAmplitude = Mathf.Min(currentValue*amplitudeMultiplier, peakAmplitude);
			float targetFrequency = Mathf.Min(currentValue*frequencyMultiplier, peakFrequency);
			float targetPhase = _mainSineWave.phase;

			float t = Time.deltaTime*(1f+i/2f);

			_sineWaves[i].amplitude = Mathf.Lerp(currentAmplitude, targetAmplitude, t);
			_sineWaves[i].frequency = Mathf.Lerp(currentFrequency, targetFrequency, t);
			_sineWaves[i].phase = Mathf.Lerp(currentPhase, targetPhase, t);
		}

		float diff = Mathf.Abs(currentValue - targetValue);

		if (diff < 0.5f)
		{
			if (_audioSource.clip != correctChimeSFX && !_confirmed)
			{
				_audioSource.clip = correctChimeSFX;
				_audioSource.Play();
			}
			_checkAccuracyCoroutine ??= StartCoroutine(CheckAccuracy());

			_tutorialLabel.text = "Calibrating...";
		}
		else
		{
			if (_audioSource.clip != angelicChoirSFX)
			{
				_audioSource.clip = angelicChoirSFX;
				_audioSource.Play();
			}

			_tutorialLabel.text = "Tune frequency to calibrate the void watch using (MOUSE WHEEL UP/DOWN)";
		}

		_audioSource.volume = Mathf.Lerp(_audioSource.volume, Mathf.Max(0f, 0.8f - (diff/4f)), Time.deltaTime*3f);

		if (_confirmed)
		{
			_opacity = Mathf.Lerp(_opacity, 0f, Time.deltaTime*3f);
			_root.style.opacity = _opacity;
		}

		if (_opacity < 0.03f)
		{
			_opacity = 0f;
			_root.style.opacity = _opacity;
			GameManager.Instance?.GetPlayer()?.EnableMoving();
			_root.style.display = DisplayStyle.None;
			SceneSwitcher.SwitchScene(GameManager.Instance.currentScene switch
			{
				GameManager.Scenes.CITY_VOID => "CityVoid",
				GameManager.Scenes.APARTMENT_VOID => "ApartmentVoid",
				_ => null
			});
			enabled = false;
		}
	}
	
	IEnumerator CheckAccuracy()
	{
		yield return new WaitForSeconds(3f);

		if (Mathf.Abs(currentValue - targetValue) < 0.5f)
		{
			_confirmed = true;
			_audioSource.clip = successChimeSFX;
			_audioSource.loop = false;
			_audioSource.Play();
		}
		else 
		{
			_checkAccuracyCoroutine = null;
		}
	}
}
