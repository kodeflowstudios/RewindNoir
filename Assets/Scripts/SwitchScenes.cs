using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public class SwitchScenes : MonoBehaviour
{
	public Label skipLabel;
	public UIDocument cutsceneDocument;
	public Coroutine skipCoroutine;
	public InputActionReference skipAction;
	public PlayableDirector timelineDirector;

	void Start()
	{
		skipLabel = cutsceneDocument?.rootVisualElement.Q<Label>("label_skip");
	}

	void OnEnable()
	{
		skipAction.action.performed += StartSkipCutscene;
		skipAction.action.canceled += CancelSkipCutscene;
	}

    void OnDisable()
	{
		skipAction.action.performed -= StartSkipCutscene;
		skipAction.action.canceled -= CancelSkipCutscene;
	}

    private void StartSkipCutscene(InputAction.CallbackContext context)
    {
		skipLabel.style.display = DisplayStyle.Flex;
		skipCoroutine = StartCoroutine(SkipCutscene());
    }

    private void CancelSkipCutscene(InputAction.CallbackContext context)
    {
		StopCoroutine(skipCoroutine);
		skipLabel.style.display = DisplayStyle.None;
    }

	IEnumerator SkipCutscene()
	{
		yield return new WaitForSeconds(3);
		timelineDirector.Pause();
		Switch();
	}

	public void Switch()
	{
		SceneSwitcher.SwitchScene("City");
	}
}
