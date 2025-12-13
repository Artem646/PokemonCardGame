using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ParallelogramButton : Button
{
    [UxmlAttribute] public string TargetSceneName { get; set; } = "";
    public Color FillColor { get; set; } = Color.clear;
    public Color BorderColor { get; set; } = Color.whiteSmoke;
    public float BorderWidth { get; set; } = 2f;
    public float ActiveSceneBorderWidth { get; set; } = 4f;
    public float SkewOffset { get; set; } = 30f;
    public Color HoverColor { get; set; } = new Color(0.3f, 0.8f, 0.4f, 0.3f);
    public Color PressedColor { get; set; } = new Color(0.1f, 0.4f, 0.2f);
    public Color ActiveSceneBorderColor { get; set; } = Color.aquamarine;

    private bool isHovered = false;
    private bool isPressed = false;

    public ParallelogramButton()
    {
        generateVisualContent += OnGenerateVisualContent;

        RegisterCallback<MouseEnterEvent>(evt => { isHovered = true; MarkDirtyRepaint(); });
        RegisterCallback<MouseLeaveEvent>(evt => { isHovered = false; isPressed = false; MarkDirtyRepaint(); });
        RegisterCallback<MouseDownEvent>(evt => { isPressed = true; MarkDirtyRepaint(); });
        RegisterCallback<MouseUpEvent>(evt => { isPressed = false; MarkDirtyRepaint(); });
    }

    private void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        var painter = ctx.painter2D;
        float w = resolvedStyle.width;
        float h = resolvedStyle.height;
        float halfBorder = BorderWidth / 2f;

        Color currentColor = FillColor;
        if (isPressed) currentColor = PressedColor;
        else if (isHovered) currentColor = HoverColor;

        bool isSceneActive = SceneManager.GetActiveScene().name == TargetSceneName;

        painter.fillColor = currentColor;
        painter.strokeColor = isSceneActive ? ActiveSceneBorderColor : BorderColor;
        painter.lineWidth = isSceneActive ? ActiveSceneBorderWidth : BorderWidth;

        painter.BeginPath();
        painter.MoveTo(new Vector2(SkewOffset + halfBorder, halfBorder));
        painter.LineTo(new Vector2(w - halfBorder, halfBorder));
        painter.LineTo(new Vector2(w - SkewOffset - halfBorder, h - halfBorder));
        painter.LineTo(new Vector2(halfBorder, h - halfBorder));
        painter.ClosePath();
        painter.Fill();
        painter.Stroke();
    }
}