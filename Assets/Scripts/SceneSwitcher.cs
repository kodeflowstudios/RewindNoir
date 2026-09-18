using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
	public static void SwitchScene(string sceneName)
	{
		if (string.IsNullOrEmpty(sceneName)) return;

		var loading = SceneManager.LoadSceneAsync(sceneName);
		if (loading == null)
		{
			Debug.LogError($"Failed to load scene: {sceneName}");
			return;
		}

		if (GameManager.Instance == null) return;

		GameManager.Instance.currentScene = sceneName switch
		{
			"MainMenu" => GameManager.Scenes.MAIN_MENU,
			"Intro" => GameManager.Scenes.INTRO,
			"City" => GameManager.Scenes.CITY,
			"CityVoid" => GameManager.Scenes.CITY_VOID,
			"Apartment" => GameManager.Scenes.APARTMENT,
			"ApartmentVoid" => GameManager.Scenes.APARTMENT_VOID,
			_ => GameManager.Instance.currentScene
		};
	}
}
