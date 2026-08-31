using TMPro;
using UnityEngine;

public class UIComponent : MonoBehaviour
{
	public UIManager.Menu menu;
	public string key;

    void Start()
    {
		GetComponent<TMP_Text>().text = UIManager.Instance.GetUIText(menu, key);
    }
}
