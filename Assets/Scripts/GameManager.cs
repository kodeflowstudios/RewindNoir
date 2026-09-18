using System.Collections;
using KodeFlowStudios.Parley.Localization;
using KodeFlowStudios.Parley.YamlCore;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public enum Scenes
	{
		MAIN_MENU,
		INTRO,
		FM_SINES,
		CITY,
		CITY_VOID,
		APARTMENT,
		APARTMENT_VOID
	}

	public Scenes currentScene = Scenes.CITY;

	public bool hasTalked = false;
	public bool inDialogue = false;
	public bool enteredEntropy = false;
	public bool hasOpenedNotepad = false;
	public bool choseRight = false;

	public float _duration = 0.5f;
	public float _fovMin = 60f;
	public float _fovMax = 300f;

	public GameObject playerA;
	public GameObject playerB;
	public Vector3 positionA;
	public Vector3 positionB;
	public Quaternion camRotationA;
	public Quaternion camRotationB;
	public ObjectiveUpdater obu;
	public CinemachinePanTilt cinePanTiltA;
	public CinemachinePanTilt cinePanTiltB;
	public InputAxis cineCamAPan;
	public InputAxis cineCamBPan;
	public InputAxis cineCamATilt;
	public InputAxis cineCamBTilt;
	public CinemachineCamera cineCamA;
	public CinemachineCamera cineCamB;
	public Image fade;
	public AnimationCurve animationCurve;
	public ParleyYaml npcDialogue;
	public ParleyYaml objectivesDialogue;

	private bool _isTuned = false;

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
		objectivesDialogue = new ParleyYaml("Misc", "Objectives", Localizer.GetIDFromEnglishName(PlayerPrefs.GetString("Language")));
	}

	public PlayerController GetPlayer()
	{
		if (playerA != null) return playerA.GetComponent<PlayerController>();
		if (playerB != null) return playerB.GetComponent<PlayerController>();
		return null;
	}

	public void TransitionToNormal()
	{
		StartCoroutine(TransitionToNormalEnumerator());
	}

	private IEnumerator TransitionToNormalEnumerator()
	{
		obu.HideObjective();

		positionB = playerB.transform.position;
		cineCamBPan = cinePanTiltB.PanAxis;
		cineCamBTilt = cinePanTiltB.TiltAxis;

		Color c = fade.color;
		float timeElapsed = 0;

		while (timeElapsed < _duration)
		{
			float t = timeElapsed/_duration;

			t = animationCurve.Evaluate(1-t);

			cineCamB.Lens.FieldOfView = Mathf.Lerp(_fovMax, _fovMin, t);
			c.a = Mathf.Lerp(1, 0, t);
			fade.color = c;
			timeElapsed += Time.deltaTime;

			yield return null;
		}

		cineCamB.Lens.FieldOfView = _fovMax;
		c.a = 1;
		fade.color = c;

		SceneSwitcher.SwitchScene(currentScene switch
		{
			Scenes.CITY_VOID => "City",
			Scenes.APARTMENT_VOID => "Apartment",
			_ => null
		});

		while (!playerA) yield return new WaitForEndOfFrame();

		var charCont = playerA.GetComponent<CharacterController>();
		charCont.enabled = false;
		playerA.transform.position = positionB;
		charCont.enabled = true;

		while (!cinePanTiltA) yield return new WaitForEndOfFrame();
		cinePanTiltA.enabled = false;
		cinePanTiltA.PanAxis = cineCamBPan;
		cinePanTiltA.TiltAxis = cineCamBTilt;
		cinePanTiltA.enabled = true;

		timeElapsed = 0;

		while (timeElapsed < _duration)
		{
			float t = timeElapsed/_duration;

			t = animationCurve.Evaluate(t);

			cineCamA.Lens.FieldOfView = Mathf.Lerp(_fovMax, _fovMin, t);
			c.a = Mathf.Lerp(1, 0, t);
			fade.color = c;
			timeElapsed += Time.deltaTime;

			yield return null;
		}

		cineCamA.Lens.FieldOfView = _fovMin;
		c.a = 0;
		fade.color = c;

		obu.ShowObjective();
	}

	public void TransitionToVoid()
	{
		StartCoroutine(TransitionToVoidEnumerator());
	}

	private IEnumerator TransitionToVoidEnumerator()
	{
		obu.HideObjective();

		positionA = playerA.transform.position;
		cineCamAPan = cinePanTiltA.PanAxis;
		cineCamATilt = cinePanTiltA.TiltAxis;

		Color c = fade.color;
		float timeElapsed = 0;

		while (timeElapsed < _duration)
		{
			float t = timeElapsed/_duration;

			t = animationCurve.Evaluate(1-t);

			cineCamA.Lens.FieldOfView = Mathf.Lerp(_fovMax, _fovMin, t);
			c.a = Mathf.Lerp(1, 0, t);
			fade.color = c;
			timeElapsed += Time.deltaTime;

			yield return null;
		}

		cineCamA.Lens.FieldOfView = _fovMax;
		c.a = 1;
		fade.color = c;

		SceneSwitcher.SwitchScene(currentScene switch
		{
			Scenes.CITY => _isTuned ? "CityVoid" : "FMSines",
			Scenes.APARTMENT => _isTuned ? "ApartmentVoid" : "FMSines",
			_ => null
		});

		currentScene = currentScene switch 
		{
			Scenes.CITY => Scenes.CITY_VOID,
			Scenes.APARTMENT => Scenes.APARTMENT_VOID,
			_ => currentScene
		};

		if (!enteredEntropy)
		{
			obu.UpdateObjective("entropy_tutorial");
			obu.UpdateObjective();
			enteredEntropy = true;
		}

		while (!playerB) yield return new WaitForEndOfFrame();
		var charCont = playerB.GetComponent<CharacterController>();
		charCont.enabled = false;
		playerB.transform.position = positionA;
		charCont.enabled = true;

		while (!cinePanTiltB) yield return new WaitForEndOfFrame();
		cinePanTiltB.enabled = false;
		cinePanTiltB.PanAxis = cineCamAPan;
		cinePanTiltB.TiltAxis = cineCamATilt;
		cinePanTiltB.enabled = true;

		timeElapsed = 0;

		while (timeElapsed < _duration)
		{
			float t = timeElapsed/_duration;

			t = animationCurve.Evaluate(t);

			cineCamB.Lens.FieldOfView = Mathf.Lerp(_fovMax, _fovMin, t);
			c.a = Mathf.Lerp(1, 0, t);
			fade.color = c;
			timeElapsed += Time.deltaTime;

			yield return null;
		}

		cineCamB.Lens.FieldOfView = _fovMin;
		c.a = 0;
		fade.color = c;

		obu.ShowObjective();

		_isTuned = true;
	}

    public void LoadDialogue(string folderName, string fileName)
    {
		npcDialogue = new ParleyYaml(folderName, fileName, Localizer.GetIDFromEnglishName(PlayerPrefs.GetString("Language")));
    }
}
