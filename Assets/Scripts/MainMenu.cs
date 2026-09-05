using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
		UIDocument doc = GetComponent<UIDocument>();
		var root = doc.rootVisualElement;
    }
}
