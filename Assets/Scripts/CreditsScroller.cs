using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class CreditsScroller : MonoBehaviour
{
    [SerializeField] float speed = 60f; // pixels per second
	[SerializeField] TextAsset creditsText;

    ScrollView scroll;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        scroll = root.Q<ScrollView>("credits-scroll");
        var label = root.Q<Label>("credits-label");

		label.enableRichText = true;
		label.text = creditsText.text;

        scroll.pickingMode = PickingMode.Ignore;

        scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            float h = evt.newRect.height;
            label.style.paddingTop = h;
            label.style.paddingBottom = h;
        });

        scroll.scrollOffset = Vector2.zero;
    }

    void Update()
    {
        float max = scroll.contentContainer.layout.height - scroll.contentViewport.layout.height;
        if (max <= 0f) return;

        var offset = scroll.scrollOffset;
        offset.y = Mathf.Min(offset.y + speed * Time.unscaledDeltaTime, max);
        scroll.scrollOffset = offset;

        if (offset.y >= max)
        {
            enabled = false;
        }
    }
}
