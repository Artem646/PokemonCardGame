// using UnityEngine;
// using UnityEngine.UIElements;

// public class CardCollectionManager : MonoBehaviour
// {
//     [SerializeField] private UIDocument uiDocument;
//     [SerializeField] private UIDocument uiCard;
//     private VisualElement root;
//     private VisualElement overlay;
//     private VisualElement card;

//     private void Start()
//     {
//         root = uiDocument.rootVisualElement;
//         cardRoot = uiDocument.rootVisualElement;
//         overlay = root.Q<VisualElement>("overlay");
//         card = root.Q<VisualElement>("fullCard");
//         card.RegisterCallback<ClickEvent>(evt =>
//         {
//             overlay.Clear();
//             overlay.Add(card);
//             overlay.AddToClassList("overlay-active");
//             card.AddToClassList("card-zoom");
//         });

//         overlay.RegisterCallback<ClickEvent>(evt =>
//         {
//             if (evt.target == overlay)
//             {
//                 card.RemoveFromClassList("card-zoom");
//                 overlay.RemoveFromClassList("overlay--active");
//                 root.Add(card);
//             }
//         });
//     }
// }