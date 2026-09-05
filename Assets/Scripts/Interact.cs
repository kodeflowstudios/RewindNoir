using UnityEngine;
using UnityEngine.InputSystem;

public class Interact : MonoBehaviour
{
	public Transform cam;
	public LayerMask hitMask;
	[SerializeField] private InputActionReference interactAction;

	private void OnEnable()
	{
		interactAction.action.performed += OnInteract;
		interactAction.action.canceled += OnInteract;
	}

	private void OnDisable()
	{
		interactAction.action.performed -= OnInteract;
		interactAction.action.canceled -= OnInteract;
	}

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (Physics.Raycast(cam.position, cam.TransformDirection(Vector3.forward), out RaycastHit hit, 10f, hitMask))
        {
			hit.collider.gameObject.GetComponent<NPC>().StartDialogue();
        }
    }
}
