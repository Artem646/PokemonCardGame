using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ParallelogramButton : Button
{
    public Color FillColor { get; set; } = Color.clear;
    public Color BorderColor { get; set; } = Color.whiteSmoke;
    public float BorderWidth { get; set; } = 2f;
    public float SkewOffset { get; set; } = 20f;
    public Color HoverColor { get; set; } = new Color(0.3f, 0.8f, 0.4f, 0.3f);
    public Color PressedColor { get; set; } = new Color(0.1f, 0.4f, 0.2f);

    private bool isHovered = false;
    private bool isPressed = false;

    public ParallelogramButton()
    {
        generateVisualContent += OnGenerateVisualContent;

        RegisterCallback<MouseEnterEvent>(evt =>
        {
            isHovered = true;
            MarkDirtyRepaint();
        });

        RegisterCallback<MouseLeaveEvent>(evt =>
        {
            isHovered = false;
            isPressed = false;
            MarkDirtyRepaint();
        });

        RegisterCallback<MouseDownEvent>(evt =>
        {
            isPressed = true;
            MarkDirtyRepaint();
        });

        RegisterCallback<MouseUpEvent>(evt =>
        {
            isPressed = false;
            MarkDirtyRepaint();
        });
    }

    private void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        var painter = ctx.painter2D;

        Color currentColor = FillColor;
        if (isPressed)
        {
            currentColor = PressedColor;
        }
        else if (isHovered)
        {
            currentColor = HoverColor;
        }

        float w = resolvedStyle.width;
        float h = resolvedStyle.height;

        painter.fillColor = currentColor;
        painter.BeginPath();
        painter.MoveTo(new Vector2(SkewOffset, 0));
        painter.LineTo(new Vector2(w, 0));
        painter.LineTo(new Vector2(w - SkewOffset, h));
        painter.LineTo(new Vector2(0, h));
        painter.ClosePath();
        painter.Fill();
        painter.Stroke();

        if (BorderWidth > 0f)
        {
            painter.strokeColor = BorderColor;
            painter.lineWidth = BorderWidth;
            painter.BeginPath();
            painter.MoveTo(new Vector2(SkewOffset, 0));
            painter.LineTo(new Vector2(w, 0));
            painter.LineTo(new Vector2(w - SkewOffset, h));
            painter.LineTo(new Vector2(0, h));
            painter.ClosePath();
            painter.Stroke();
        }
    }
}
