using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ParallelogramContainer : VisualElement
{
    public Color BorderColor { get; set; } = Color.white;
    public float BorderWidth { get; set; } = 2f;
    public float SkewOffset { get; set; } = 60f;

    public ParallelogramContainer()
    {
        generateVisualContent += OnGenerateVisualContent;
    }

    private void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        var painter = ctx.painter2D;
        float w = resolvedStyle.width;
        float h = resolvedStyle.height;

        painter.strokeColor = BorderColor;
        painter.lineWidth = BorderWidth;

        painter.BeginPath();
        painter.MoveTo(new Vector2(0, 0));
        painter.LineTo(new Vector2(w, 0));
        painter.LineTo(new Vector2(w, h));
        painter.LineTo(new Vector2(SkewOffset, h));
        painter.ClosePath();
        painter.Stroke();

        // // Верхняя линий
        // painter.strokeColor = BorderColor;
        // painter.lineWidth = BorderWidth;
        // painter.BeginPath();
        // painter.MoveTo(new Vector2(0, 0));
        // painter.LineTo(new Vector2(w, 0));
        // painter.Stroke();

        // // Правая линия
        // painter.strokeColor = BorderColor;
        // painter.lineWidth = BorderWidth;
        // painter.BeginPath();
        // painter.MoveTo(new Vector2(w, 0));
        // painter.LineTo(new Vector2(w, h));
        // painter.Stroke();

        // // Нижняя линия
        // painter.strokeColor = BorderColor;
        // painter.lineWidth = BorderWidth;
        // painter.BeginPath();
        // painter.MoveTo(new Vector2(w, h));
        // painter.LineTo(new Vector2(SkewOffset, h));
        // painter.Stroke();

        // // Левая линия
        // painter.strokeColor = BorderColor;
        // painter.lineWidth = BorderWidth;
        // painter.BeginPath();
        // painter.MoveTo(new Vector2(SkewOffset, h));
        // painter.LineTo(new Vector2(0, 0));
        // painter.Stroke();
    }
}
