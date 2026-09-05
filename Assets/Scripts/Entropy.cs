using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class Entropy : MonoBehaviour
{
	public bool inVoid = false;
	[SerializeField] private InputActionReference entropyAction;

	void Start()
	{
		if (inVoid)
		{
			if (!GameManager.Instance.playerB) 
			{
				GameManager.Instance.playerB = gameObject;
			}
			if (!GameManager.Instance.cineCamB)
			{
				GameManager.Instance.cineCamB = GetComponentInChildren<CinemachineCamera>();
				GameManager.Instance.cinePanTiltB = GetComponentInChildren<CinemachinePanTilt>();
			}
		}
		else
		{
			if (!GameManager.Instance.playerA) 
			{
				GameManager.Instance.playerA = gameObject;
			}
			if (!GameManager.Instance.cineCamA)
			{
				GameManager.Instance.cineCamA = GetComponentInChildren<CinemachineCamera>();
				GameManager.Instance.cinePanTiltA = GetComponentInChildren<CinemachinePanTilt>();
			}
		}
	}

	void OnEnable()
	{
		entropyAction.action.performed += ToggleVoid;
	}

    void OnDisable()
	{
		entropyAction.action.performed -= ToggleVoid;
	}

    private void ToggleVoid(InputAction.CallbackContext context)
    {
		if (inVoid) 
		{
			GameManager.Instance.TransitionToNormal();
		}
		else 
		{
			GameManager.Instance.TransitionToVoid();
		}
    }
}
