using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class CustomizableButton : Button
{
    [UxmlAttribute] public Color FillColor { get; set; } = Color.clear;
    [UxmlAttribute] public Color HoverColor { get; set; } = Color.chocolate;
    [UxmlAttribute] public Color PressedColor { get; set; } = Color.chocolate;
    [UxmlAttribute] public Color BorderColor { get; set; } = Color.whiteSmoke;
    [UxmlAttribute] public float BorderWidth { get; set; } = 2f;

    [UxmlAttribute] public float AngleTopLeft { get; set; } = 0f;
    [UxmlAttribute] public float AngleTopRight { get; set; } = 0f;
    [UxmlAttribute] public float AngleBottomLeft { get; set; } = 0f;
    [UxmlAttribute] public float AngleBottomRight { get; set; } = 0f;

    [UxmlAttribute] public float AngleLeftTop { get; set; } = 0f;
    [UxmlAttribute] public float AngleLeftBottom { get; set; } = 0f;
    [UxmlAttribute] public float AngleRightTop { get; set; } = 0f;
    [UxmlAttribute] public float AngleRightBottom { get; set; } = 0f;
    [UxmlAttribute] public string ButtonText { get; set; } = "PLAY";

    private Label textLabel;

    private bool isHovered = false;
    private bool isPressed = false;

    public CustomizableButton()
    {
        generateVisualContent += OnGenerateVisualContent;

        textLabel = new Label();
        textLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        textLabel.style.flexGrow = 1;
        textLabel.style.color = Color.white;
        Add(textLabel);

        RegisterCallback<GeometryChangedEvent>(evt => { textLabel.text = ButtonText; });

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

        painter.fillColor = currentColor;
        painter.strokeColor = BorderColor;
        painter.lineWidth = BorderWidth;

        float radTL = AngleTopLeft * Mathf.Deg2Rad;
        float radTR = AngleTopRight * Mathf.Deg2Rad;
        float radBL = AngleBottomLeft * Mathf.Deg2Rad;
        float radBR = AngleBottomRight * Mathf.Deg2Rad;

        float radLT = AngleLeftTop * Mathf.Deg2Rad;
        float radLB = AngleLeftBottom * Mathf.Deg2Rad;
        float radRT = AngleRightTop * Mathf.Deg2Rad;
        float radRB = AngleRightBottom * Mathf.Deg2Rad;

        Vector2 topLeft = new(
            halfBorder + Mathf.Tan(radTL) * h,
            halfBorder + Mathf.Tan(radLT) * w
        );

        Vector2 topRight = new(
            w - halfBorder + Mathf.Tan(radTR) * h,
            halfBorder + Mathf.Tan(radRT) * w
        );

        Vector2 bottomRight = new(
            w - halfBorder + Mathf.Tan(radBR) * h,
            h - halfBorder + Mathf.Tan(radRB) * w
        );

        Vector2 bottomLeft = new(
            halfBorder + Mathf.Tan(radBL) * h,
            h - halfBorder + Mathf.Tan(radLB) * w
        );

        painter.BeginPath();
        painter.MoveTo(topLeft);
        painter.LineTo(topRight);
        painter.LineTo(bottomRight);
        painter.LineTo(bottomLeft);
        painter.ClosePath();
        painter.Fill();
        painter.Stroke();
    }
}
