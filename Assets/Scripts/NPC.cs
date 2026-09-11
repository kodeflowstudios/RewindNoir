using UnityEngine;

public class NPC : MonoBehaviour
{
	public Transform player;
	public ObjectiveUpdater obu;

	public void LookAtPlayer()
	{
		var lookPos = player.position - transform.position;
		lookPos.y = 0;
		transform.rotation = Quaternion.LookRotation(lookPos);
	}
}
