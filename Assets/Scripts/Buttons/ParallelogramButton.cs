using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ParallelogramButton : Button
{
    [UxmlAttribute] public string TargetSceneName { get; set; } = "";
    [UxmlAttribute] public Color FillColor { get; set; } = Color.clear;
    [UxmlAttribute] public Color BorderColor { get; set; } = Color.whiteSmoke;
    [UxmlAttribute] public float BorderWidth { get; set; } = 2f;
    [UxmlAttribute] public float ActiveSceneBorderWidth { get; set; } = 4f;
    [UxmlAttribute] public float SkewOffset { get; set; } = 30f;
    [UxmlAttribute] public Color HoverColor { get; set; } = new Color(0.3f, 0.8f, 0.4f, 0.3f);
    [UxmlAttribute] public Color PressedColor { get; set; } = new Color(0.1f, 0.4f, 0.2f);
    [UxmlAttribute] public Color ActiveSceneBorderColor { get; set; } = Color.aquamarine;

    private CustomStyleProperty<Color> FillColorUSS = new("--parallelogram-btn-fill-color");
    private CustomStyleProperty<Color> HoverColorUSS = new("--parallelogram-btn-hover-color");
    private CustomStyleProperty<Color> BorderColorUSS = new("--parallelogram-btn-border-color");
    private CustomStyleProperty<Color> ActiveBorderColorUSS = new("--parallelogram-btn-active-color");

    private bool isHovered = false;
    private bool isPressed = false;

    public ParallelogramButton()
    {
        generateVisualContent += OnGenerateVisualContent;

        RegisterCallback<MouseEnterEvent>(evt => { isHovered = true; MarkDirtyRepaint(); });
        RegisterCallback<MouseLeaveEvent>(evt => { isHovered = false; isPressed = false; MarkDirtyRepaint(); });
        RegisterCallback<MouseDownEvent>(evt => { isPressed = true; MarkDirtyRepaint(); });
        RegisterCallback<MouseUpEvent>(evt => { isPressed = false; MarkDirtyRepaint(); });

        RegisterCallback<CustomStyleResolvedEvent>(evt => MarkDirtyRepaint());
    }

    private void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        var painter = ctx.painter2D;
        float w = resolvedStyle.width;
        float h = resolvedStyle.height;
        float halfBorder = BorderWidth / 2f;

        if (customStyle.TryGetValue(FillColorUSS, out var fillColor)) FillColor = fillColor;
        if (customStyle.TryGetValue(HoverColorUSS, out var hoverColor)) HoverColor = hoverColor;
        if (customStyle.TryGetValue(BorderColorUSS, out var borderColor)) BorderColor = borderColor;
        if (customStyle.TryGetValue(ActiveBorderColorUSS, out var activeSceneBorderColor)) ActiveSceneBorderColor = activeSceneBorderColor;

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