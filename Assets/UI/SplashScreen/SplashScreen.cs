using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SplashScreen : MonoBehaviour
{
	public AnimationCurve animationCurve;
	InputAction _skipAction;
	Coroutine _splashCoroutine;
	VisualElement _root;

	void OnEnable()
	{
		_skipAction = new InputAction("Skip", binding: "<Mouse>/leftButton");
		_skipAction.AddBinding("<Keyboard>/escape");
		_skipAction.AddBinding("<Keyboard>/enter");
		_skipAction.performed += Skip;
		_skipAction.Enable();

		_splashCoroutine = StartCoroutine(Splash());
	}

	void OnDisable()
	{
		_skipAction.performed -= Skip;
		_skipAction.Dispose();
		_skipAction.Disable();
	}

    private void Skip(InputAction.CallbackContext context)
    {
		StopCoroutine(_splashCoroutine);
		SceneManager.LoadScene(1);
    }

    IEnumerator Splash()
	{
		_root = GetComponent<UIDocument>().rootVisualElement;
		var splash = _root.Q<VisualElement>("container_splash");

		float timeElapsed = 0;

		while (timeElapsed < 1)
		{
			float t = timeElapsed/1;

			t = animationCurve.Evaluate(t);

			splash.style.opacity = Mathf.Lerp(0, 1, t);

			timeElapsed += Time.deltaTime;

			yield return null;
		}

		splash.style.opacity = 1f;

		yield return new WaitForSeconds(3f);

		timeElapsed = 0;

		while (timeElapsed < 1)
		{
			float t = timeElapsed/1;

			t = animationCurve.Evaluate(t);

			splash.style.opacity = Mathf.Lerp(1, 0, t);

			timeElapsed += Time.deltaTime;

			yield return null;
		}

		splash.style.opacity = 0;

		yield return new WaitForSeconds(0.5f);

		SceneManager.LoadScene(1);
	}
}
