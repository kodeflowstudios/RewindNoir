using UnityEngine;

public class RotateLookAt : MonoBehaviour
{
	public Transform player;
	public GameObject operand;
	private Transform _prev;

	public void LookAtPlayer()
	{
		_prev = operand.transform;
		var lookPos = player.position - operand.transform.position;
		lookPos.y = 0;
		operand.transform.rotation = Quaternion.LookRotation(lookPos);
	}

	public void Reset()
	{
		transform.rotation = _prev.rotation;
	}
}
