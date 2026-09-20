using UnityEngine;

public class PlaySFX : MonoBehaviour
{
	public AudioClip openClip;
	public AudioClip closeClip;
	public AudioSource source;
	public Door door;

	void OnEnable()
	{
		if (door != null) source.clip = door.isOpen ? openClip : closeClip;
		else source.clip = openClip;

		source.Play();
	}
}
