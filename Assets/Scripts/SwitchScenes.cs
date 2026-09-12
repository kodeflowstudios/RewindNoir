using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScenes : MonoBehaviour
{
	public void Switch()
	{
		SceneManager.LoadScene(3);
	}
}
