// using UnityEngine.UIElements;
// using System;
// using UnityEngine;

// public enum CardControllerMode
// {
//     CollectionUIToolkit,
//     BattleUGUI
// }


// public class CardControllerBuilder
// {
//     private readonly CardControllerMode cardControllerMode;
//     private readonly VisualTreeAsset uxmlTemplate;
//     private readonly GameObject uguiPrefab;
//     private readonly Transform uguiParent;

//     public CardControllerBuilder(CardControllerMode mode, VisualTreeAsset template = null, GameObject prefab = null, Transform parent = null)
//     {
//         cardControllerMode = mode;
//         uxmlTemplate = template;
//         uguiPrefab = prefab;
//         uguiParent = parent;
//     }

//     public BaseCardController Build(CardModel model)
//     {
//         return cardControllerMode switch
//         {
//             CardControllerMode.CollectionUIToolkit =>
//                 new CollectionCardController(
//                     model,
//                     new CardViewUIToolkit(model, uxmlTemplate),
//                     this),

//             CardControllerMode.BattleUGUI =>
//                 new BattleCardController(
//                     model,
//                     new CardViewUGUI(model, uguiPrefab, uguiParent)),

//             _ => throw new NotImplementedException()
//         };
//     }
// }
