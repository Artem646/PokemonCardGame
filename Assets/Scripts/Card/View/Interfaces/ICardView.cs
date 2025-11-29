using UnityEngine;
using UnityEngine.UIElements;

public interface ICardView
{
    GameObject CardRootGameObject { get; }
    VisualElement CardRootUIToolkit { get; }
}
