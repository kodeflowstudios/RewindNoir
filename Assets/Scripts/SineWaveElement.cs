using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class SineWaveElement : VisualElement
{
    [UxmlAttribute]
    public float amplitude = 40f;

    [UxmlAttribute]
    public float frequency = 2f;

    [UxmlAttribute]
    public float phase = 0f;

    [UxmlAttribute]
    public float lineWidth = 2f;

    [UxmlAttribute]
    public float timeMultiplier = 0f;

    IVisualElementScheduledItem _animLoop;

    public SineWaveElement()
    {
        generateVisualContent += OnGenerateVisualContent;

		RegisterCallback<AttachToPanelEvent>(OnAttach);
        RegisterCallback<DetachFromPanelEvent>(OnDetach);
    }

	void OnAttach(AttachToPanelEvent evt)
    {
        _animLoop = schedule.Execute(AnimateStep).Every(16);
    }

    void OnDetach(DetachFromPanelEvent evt)
    {
        _animLoop?.Pause();
    }

    void AnimateStep(TimerState ts)
    {
        phase += (ts.deltaTime / 1000f) * timeMultiplier;
        MarkDirtyRepaint();
    }

	void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        var rect = contentRect;
        if (rect.width < 1 || rect.height < 1) return;

        var painter = mgc.painter2D;
		painter.strokeColor = resolvedStyle.color;
        painter.lineWidth = lineWidth;

        painter.BeginPath();

        float midY = rect.height / 2f;
        int steps = Mathf.Max(2, (int)rect.width);

        for (int i = 0; i <= steps; i++)
        {
            float x = (i / (float)steps) * rect.width;
            float t = (i / (float)steps) * frequency * Mathf.PI * 2f;
            float y = midY + Mathf.Sin(t + phase) * amplitude;

            if (i == 0)
                painter.MoveTo(new Vector2(x, y));
            else
                painter.LineTo(new Vector2(x, y));
        }

        painter.Stroke();
    }
}
