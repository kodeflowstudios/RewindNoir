using UnityEngine;

public class SunSceneSwitch : MonoBehaviour
{
	void OnEnable()
	{
		SceneSwitcher.SwitchScene("EndScreen");
	}
}
