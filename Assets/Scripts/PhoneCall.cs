using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PhoneCall : MonoBehaviour
{
	public PlayerController playerController;
	public InputActionReference acceptAction;
	public InputActionReference toggleAction;
	public AudioClip vibrateSfx;
	public AudioClip ringingSfx;
	public AudioClip hangUpSfx;
	public AudioSource audioSource1;
	public AudioSource audioSource2;
	public AnimationCurve animationCurve;
	private VisualElement _root;
	private VisualElement _phone;
	private Label _hintLabel;
	private float _slideDuration = 0.3f;
	private bool _isShown = false;
	private Coroutine _ringCoroutine;
	private ObjOfInterest _sequence;
	private bool _canAnswer = false;

    void Start()
    {
		_root = GetComponent<UIDocument>().rootVisualElement;

		_phone = _root.Q<VisualElement>("phone");
		_hintLabel = _root.Q<Label>("label_hint");

		_root.style.display = DisplayStyle.None;

		_sequence = GetComponent<ObjOfInterest>();

		Invoke("StartRinging", 8f);
    }

	void StartRinging()
	{
		_ringCoroutine = StartCoroutine(RingAnimation());
	}

	IEnumerator RingAnimation()
	{
		_canAnswer = true;
		audioSource1.clip = ringingSfx;
		audioSource1.loop = true;
		audioSource1.Play();
		while (true)
		{
			audioSource2.clip = vibrateSfx;
			audioSource2.Play();
			_phone.style.rotate = new Rotate(5);
			yield return new WaitForSeconds(0.1f);
			_phone.style.rotate = new Rotate(-5);
			yield return new WaitForSeconds(0.1f);
			_phone.style.rotate = new Rotate(5);
			yield return new WaitForSeconds(0.1f);
			_phone.style.rotate = new Rotate(-5);
			yield return new WaitForSeconds(0.1f);
			_phone.style.rotate = new Rotate(5);
			yield return new WaitForSeconds(0.1f);
			_phone.style.rotate = new Rotate(-5);
			yield return new WaitForSeconds(0.1f);
			_phone.style.rotate = new Rotate(5);
			yield return new WaitForSeconds(0.1f);
			_phone.style.rotate = new Rotate(-5);
			yield return new WaitForSeconds(0.1f);
			_phone.style.rotate = new Rotate(5);
			yield return new WaitForSeconds(0.1f);
			_phone.style.rotate = new Rotate(0);
			yield return new WaitForSeconds(2.2f);
		}
	}

    void OnEnable()
	{
		toggleAction.action.performed += Toggle;
		acceptAction.action.performed += Accept;
	}

    void OnDisable()
	{
		toggleAction.action.performed -= Toggle;
		acceptAction.action.performed -= Accept;
	}

    private void Accept(InputAction.CallbackContext context)
    {
		if (!_canAnswer) return;
		if (!_isShown) return;
		_sequence.StartDialogue();
		StopCoroutine(_ringCoroutine);
		_hintLabel.style.display = DisplayStyle.None;
		_phone.style.rotate = new Rotate(0);
		audioSource1.loop = false;
		audioSource1.Stop();
		audioSource2.Stop();
		acceptAction.action.performed -= Accept;
    }

	private void Toggle(InputAction.CallbackContext context)
	{
		if (GameManager.Instance.inDialogue) return;
		if (SettingsMenu.Instance.isPaused) return;

		if (!_isShown) StartSlideUp();
		else StartSlideDown();
	}

	void StartSlideUp()
	{
		StartCoroutine(SlideUp());
	}

	public void CleanUpAfterDialogue()
	{
		_root.style.display = DisplayStyle.None;
		toggleAction.action.performed -= Toggle;
		toggleAction = null;
		acceptAction = null;
	}

	IEnumerator SlideUp()
	{
		playerController.DisableMoving();

		SettingsMenu.Instance.notePadOpen = true;

		_root.style.display = DisplayStyle.Flex;

		float timeElapsed = 0;

		while (timeElapsed < _slideDuration)
		{
			float t = timeElapsed/_slideDuration;

			t = animationCurve.Evaluate(t);
			_phone.style.marginTop = new StyleLength(new Length(Mathf.Lerp(100, 0, t), LengthUnit.Percent));

			timeElapsed += Time.deltaTime;

			yield return null;
		}

		_phone.style.marginTop = 0;

		_isShown = true;

		if (GameManager.Instance.hasTalked)
		{
			if (!GameManager.Instance.hasOpenedNotepad)
			{
				GameManager.Instance.obu.HideObjective();
				GameManager.Instance.obu.UpdateObjective();
				GameManager.Instance.hasOpenedNotepad = true;
			}
		}
	}

	void StartSlideDown()
	{
		StartCoroutine(SlideDown());
	}

	IEnumerator SlideDown()
	{
		playerController.EnableMoving();

		float timeElapsed = 0;

		while (timeElapsed < _slideDuration)
		{
			float t = timeElapsed/_slideDuration;

			t = animationCurve.Evaluate(1-t);

			_phone.style.marginTop = new StyleLength(new Length(Mathf.Lerp(100, 0, t), LengthUnit.Percent));

			timeElapsed += Time.deltaTime;

			yield return null;
		}

		_phone.style.marginTop = 0;

		_root.style.display = DisplayStyle.None;

		_isShown = false;

		GameManager.Instance.obu.ShowObjective();

		SettingsMenu.Instance.notePadOpen = false;
	}
}
