using UnityEngine;

public class Door : MonoBehaviour
{
	public bool isOpen = false;
	public Animator animator;

	public void ToggleDoor()
	{
		animator.Play(isOpen ? "DoorClose" : "DoorOpen");
		isOpen = !isOpen;
	}
}
