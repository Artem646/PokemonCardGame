// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UIElements;

// public class PlaySceneController : MonoBehaviour
// {
//     [SerializeField] private UIDocument uiDocument;
//     [SerializeField] private VisualTreeAsset cardTemplate;
//     private BattleFiledCardsListController battleFiledCardsListController;
//     private VisualElement playerHand;

//     private async void Start()
//     {
//         playerHand = uiDocument.rootVisualElement.Q<VisualElement>("playerHand");
//         battleFiledCardsListController = new BattleFiledCardsListController(playerHand);
//         await battleFiledCardsListController.LoadUserCards();

//         uiDocument.rootVisualElement.Q<Button>("back").RegisterCallback<ClickEvent>(evt =>
//         {
//             SceneManager.LoadScene("CollectionScene");
//         });
//     }
// }

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PlaySceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset cardTemplate;
    private BattleFiledCardsListController battleFiledCardsListController;
    private VisualElement playerHand;

    private async void Start()
    {
        var root = uiDocument.rootVisualElement;

        playerHand = root.Q<VisualElement>("playerCardsLayer");

        battleFiledCardsListController = new BattleFiledCardsListController(playerHand);
        await battleFiledCardsListController.LoadUserCards();

        root.RegisterCallback<DropEvent>(OnDrop);

        root.Q<Button>("back").RegisterCallback<ClickEvent>(evt =>
        {
            SceneManager.LoadScene("CollectionScene");
        });
    }

    private void OnDrop(DropEvent evt)
    {
        // evt.dragger.target — это VisualElement карточки (тот, к которому применялся DragManipulator)
        // evt.droppable — VisualElement слота под курсором (тот, который помечен классом droppable)
        var card = evt.dragger.target;
        var slot = evt.droppable;

        if (card == null || slot == null) return;

        // Меняем родителя, сохраняя экранную позицию
        CardDragManipulator.ChangeParent(card, slot);

        // Дополнительно: если нужно, можно сбросить transform.position в (0,0,0) после того, как ChangeParent закончится,
        // но ChangeParent уже делает корректную установку.
    }

    // private void OnCardDropped(DropEvent evt)
    // {
    //     var card = (VisualElement)evt.target;
    //     var droppable = evt.droppable;

    //     if (droppable == null) return;

    //     // Перестановка внутри руки
    //     if (droppable.name == "playerHand")
    //     {
    //         int insertIndex = CalculateInsertIndex(droppable, card.worldBound.center.x);

    //         // Переносим карту в контейнер руки
    //         DragManipulator.ChangeParent(card, droppable);

    //         // Вставляем на нужный индекс
    //         droppable.Remove(card);
    //         droppable.Insert(insertIndex, card);

    //         // Сбрасываем смещение
    //         card.style.translate = new Translate(0, 0);
    //     }

    //     // Перенос в слот
    //     if (droppable.ClassListContains("battle-slot"))
    //     {
    //         // Переносим карту в слот
    //         DragManipulator.ChangeParent(card, droppable);

    //         // Центрируем внутри слота
    //         card.style.translate = new Translate(0, 0);
    //         card.style.alignSelf = Align.Center;
    //     }
    // }


    // private int CalculateInsertIndex(VisualElement container, float pointerX)
    // {
    //     for (int i = 0; i < container.childCount; i++)
    //     {
    //         var child = container[i];
    //         var centerX = child.worldBound.xMin + child.worldBound.width * 0.5f;
    //         if (pointerX < centerX)
    //             return i;
    //     }
    //     return container.childCount;
    // }
}
