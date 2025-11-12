// using System.Numerics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/** DragManipulator (PointerManipulator version)
    Makes a VisualElement draggable and droppable at runtime.
    Converted from original IManipulator-based version by Shane Celis.
    Author of conversion: ChatGPT (based on MIT-licensed original)
*/
public class CardDragManipulator : PointerManipulator
{
    protected static readonly CustomStyleProperty<bool> draggableEnabledProperty = new("--draggable-enabled");

    protected Vector3 offset;

    private bool isDragging = false;

    private VisualElement lastDroppable = null;

    /** USS class that identifies a valid drop target. Default = "droppable". */
    public string DroppableId { get; set; } = "droppable";

    /** Optional: disable dragging via USS custom property (--draggable-enabled: false) */
    public bool Enabled { get; set; } = true;

    private PickingMode lastPickingMode;

    private bool removedClass = false;

    /** Optional: remove this class while dragging (for disabling transitions, etc.) */
    public string RemoveClassOnDrag { get; set; } = null;

    public static Scale originalScale;

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

        target.AddToClassList("draggable");
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
    }

    private void OnCustomStyleResolved(CustomStyleResolvedEvent e)
    {
        if (e.customStyle.TryGetValue(draggableEnabledProperty, out bool got))
            Enabled = got;
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (!Enabled || evt.button != 0)
            return;

        target.AddToClassList("draggable--dragging");

        target.style.scale = new Scale(new Vector3(originalScale.value.x * 1.03f, originalScale.value.y * 1.03f, originalScale.value.z));

        if (RemoveClassOnDrag != null)
        {
            removedClass = target.ClassListContains(RemoveClassOnDrag);
            if (removedClass)
                target.RemoveFromClassList(RemoveClassOnDrag);
        }

        lastPickingMode = target.pickingMode;
        target.pickingMode = PickingMode.Ignore;

        isDragging = true;
        offset = evt.localPosition;

        Debug.Log($"[OnPointerDown] Original scale={target.style.scale.value}");

        target.CapturePointer(evt.pointerId);
        evt.StopPropagation();
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!isDragging)
            return;

        bool canDrop = CanDrop(evt.position, out VisualElement droppable);

        if (canDrop)
            droppable.RemoveFromClassList("droppable--can-drop");

        StopDrag();
        target.ReleasePointer(evt.pointerId);

        if (canDrop)
            Drop(droppable);
        else
            ResetPosition();

        evt.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent ev)
    {
        if (!isDragging || !target.HasPointerCapture(ev.pointerId))
            return;

        if (!Enabled)
        {
            StopDrag();
            return;
        }

        // Перемещаем карточку по курсору
        Vector2 delta = ev.localPosition - offset;
        var currentTranslate = target.resolvedStyle.translate;
        target.style.translate = new Translate(currentTranslate.x + delta.x, currentTranslate.y + delta.y);

        // Проверяем, есть ли слот под курсором
        if (CanDrop(ev.position, out VisualElement droppable))
        {
            Debug.Log("Droppable slot found under cursor!");

            target.AddToClassList("draggable--can-drop");

            // Подсветка нового слота
            droppable.AddToClassList("droppable--can-drop");

            // Убираем подсветку со старого слота
            if (lastDroppable != droppable)
            {
                if (lastDroppable != null)
                    Debug.Log($"Removing highlight from previous slot: {lastDroppable.name}");
                lastDroppable?.RemoveFromClassList("droppable--can-drop");
            }

            lastDroppable = droppable;
        }
        else
        {
            Debug.Log("No droppable slot under cursor");

            // Если слот не найден, убираем подсветку
            target.RemoveFromClassList("draggable--can-drop");
            if (lastDroppable != null)
                Debug.Log($"Removing highlight from previous slot: {lastDroppable.name}");
            lastDroppable?.RemoveFromClassList("droppable--can-drop");
            lastDroppable = null;
        }

        ev.StopPropagation();
    }


    // protected virtual void Drop(VisualElement droppable)
    // {
    //     ChangeParent(target, droppable);
    //     target.style.translate = new Translate(0, 0);
    //     target.style.scale = new Scale(new Vector3(0.5f, 0.5f, 1f));
    //     var e = DropEvent.GetPooled(this, droppable);
    //     e.target = target;
    //     target.schedule.Execute(() => e.target.SendEvent(e));
    // }

    protected virtual void Drop(VisualElement droppable)
    {
        Debug.Log($"[Drop] Restoring scale={originalScale.value}");
        // Переносим карту в слот
        ChangeParent(target, droppable);

        // Сбрасываем смещения
        target.style.translate = new Translate(0, 0);

        // Восстанавливаем исходный scale
        target.style.scale = originalScale;

        // Центрируем карту через flex
        target.style.position = Position.Relative;
        target.style.left = StyleKeyword.Null;
        target.style.top = StyleKeyword.Null;

        // Слот сам центрирует содержимое (justify-content, align-items)
        droppable.style.justifyContent = Justify.Center;
        droppable.style.alignItems = Align.Center;

        Debug.Log($"[Drop] Restoring scale={originalScale.value}");

        // Генерируем событие DropEvent
        var e = DropEvent.GetPooled(this, droppable);
        e.target = target;
        target.schedule.Execute(() => e.target.SendEvent(e));
    }


    public static IVisualElementScheduledItem ChangeParent(VisualElement target, VisualElement newParent)
    {
        var position_parent = target.ChangeCoordinatesTo(newParent, Vector2.zero);
        target.RemoveFromHierarchy();
        target.style.translate = new Translate(0, 0);
        newParent.Add(target);

        return target.schedule.Execute(() =>
        {
            var newPosition = position_parent - target.ChangeCoordinatesTo(newParent, Vector2.zero);
            target.RemoveFromHierarchy();
            target.style.translate = new Translate(newPosition.x, newPosition.y);
            newParent.Add(target);
        });
    }

    public virtual void ResetPosition()
    {
        target.style.translate = new Translate(0, 0);
    }

    protected virtual bool CanDrop(Vector3 position, out VisualElement droppable)
    {
        var pickedElements = new List<VisualElement>();
        target.panel.PickAll(position, pickedElements);

        foreach (VisualElement pickedElement in pickedElements)
        {
            // if (ve.ClassListContains("hand-row"))
            // {
            //     droppable = ve;
            //     Debug.Log($"Found hand slot: {ve.name}");
            //     return true;
            // }
            // Ищем первый элемент с классом droppableId
            if (pickedElement.ClassListContains(DroppableId))
            {
                droppable = pickedElement;
                Debug.Log($"Found droppable: {pickedElement.name}");
                return true;
            }
        }

        droppable = null;
        Debug.Log("No droppable found under cursor");
        return false;
    }


    private void StopDrag()
    {
        Debug.Log($"[StopDrag] Resetting scale={originalScale.value}");
        isDragging = false;
        target.RemoveFromClassList("draggable--dragging");
        target.RemoveFromClassList("draggable--can-drop");
        lastDroppable?.RemoveFromClassList("droppable--can-drop");
        lastDroppable = null;
        target.style.opacity = 1f;
        target.style.scale = originalScale;
        Debug.Log($"[StopDrag] Resetting scale={originalScale.value}");
    }
}

/** DropEvent for runtime drag & drop */
public class DropEvent : EventBase<DropEvent>
{
    public CardDragManipulator dragger { get; protected set; }
    public VisualElement droppable { get; protected set; }

    protected override void Init()
    {
        base.Init();
        LocalInit();
    }

    private void LocalInit()
    {
        bubbles = true;
        tricklesDown = false;
    }

    public static DropEvent GetPooled(CardDragManipulator dragger, VisualElement droppable)
    {
        DropEvent pooled = GetPooled();
        pooled.dragger = dragger;
        pooled.droppable = droppable;
        return pooled;
    }

    public DropEvent() => LocalInit();
}
