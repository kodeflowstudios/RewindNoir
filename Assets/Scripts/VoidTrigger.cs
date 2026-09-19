using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

public class VoidTrigger : MonoBehaviour
{
	public PlayerController playerController;
	public PlayableDirector timeline;
	public VideoPlayer videoPlayer;

	public void PrepareClip()
	{
		videoPlayer.Prepare();
	}

	public void SwitchScenes()
	{
		Destroy(GameManager.Instance);
		SceneSwitcher.SwitchScene("SpaceTime");
	}

	private void OnTriggerEnter(Collider other)
	{
		timeline.Play();
		videoPlayer.Play();
		var sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
		foreach (var source in sources)
		{
			source.Stop();
		}
		playerController.DisableMoving();
	}
}
