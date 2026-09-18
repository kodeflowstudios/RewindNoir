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
		if (GameManager.Instance.inDialogue) return;
        if (Physics.Raycast(cam.position, cam.TransformDirection(Vector3.forward), out RaycastHit hit, 10f, hitMask))
        {
			GameObject obj = hit.collider.gameObject;
			switch (obj.tag)
			{
				case "NPC":
					obj.GetComponent<DialogueHandler>()?.StartDialogue();
					break;
				case "Object of interest":
					obj.GetComponent<ObjOfInterest>()?.StartDialogue();
					break;
				case "Door":
					obj.GetComponent<Door>().ToggleDoor();
					break;
				default:
					Debug.Log($"Unkown tag found: {obj.tag}");
					break;
			}
        }
    }
}
