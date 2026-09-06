using System.Collections;
using KodeFlowStudios.Parley.YamlCore;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;
	public bool hasTalked = false;

	public float _duration = 0.5f;
	public float _fovMin = 60f;
	public float _fovMax = 300f;

	public GameObject playerA;
	public GameObject playerB;
	public Vector3 positionA;
	public Vector3 positionB;
	public Quaternion camRotationA;
	public Quaternion camRotationB;
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
	public ParleyYaml parleyYaml;

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
		parleyYaml = new ParleyYaml("Dialogues", "Cop");
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

		SceneManager.LoadScene(1);

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
	}

	public void TransitionToVoid()
	{
		StartCoroutine(TransitionToVoidEnumerator());
	}

	private IEnumerator TransitionToVoidEnumerator()
	{
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

		SceneManager.LoadScene(2);

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
	}
}
