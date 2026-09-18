using UnityEngine;

public class PlaySFX : MonoBehaviour
{
	public AudioClip openClip;
	public AudioClip closeClip;
	public AudioSource source;
	public Door door;

	void OnEnable()
	{
		source.clip = door.isOpen ? openClip : closeClip;
		source.Play();
	}
}
