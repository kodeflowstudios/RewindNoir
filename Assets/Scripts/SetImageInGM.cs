using UnityEngine;
using UnityEngine.UI;

public class SetImageInGM : MonoBehaviour
{
	void Start()
	{
		if (GameManager.Instance)
		{
			if (!GameManager.Instance.fade) GameManager.Instance.fade = GetComponent<Image>();
		}
	}
}
