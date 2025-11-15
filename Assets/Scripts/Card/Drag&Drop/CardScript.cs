using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class CardScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Camera mainCamera;
    Vector3 offset;
    public Transform DefaultParent { get; set; }
    public Transform DefaultTempCardParent { get; set; }
    GameObject tempCardGO;

    void Awake()
    {
        mainCamera = Camera.allCameras[0];
        tempCardGO = GameObject.Find("TempCardGO");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        offset = transform.position - mainCamera.ScreenToWorldPoint(eventData.position);
        DefaultParent = DefaultTempCardParent = transform.parent;

        tempCardGO.transform.SetParent(DefaultParent);
        tempCardGO.transform.SetSiblingIndex(transform.GetSiblingIndex());

        transform.SetParent(DefaultParent.parent);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 newPos = mainCamera.ScreenToWorldPoint(eventData.position);
        transform.position = newPos + offset;

        if (tempCardGO.transform.parent != DefaultTempCardParent)
        {
            tempCardGO.transform.SetParent(DefaultTempCardParent);
        }

        CheckPosition();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(DefaultParent);
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        transform.SetSiblingIndex(tempCardGO.transform.GetSiblingIndex());
        tempCardGO.transform.SetParent(GameObject.Find("Canvas").transform);
        tempCardGO.transform.localPosition = new Vector3(2600, 0);
    }

    void CheckPosition()
    {
        int newIndex = DefaultTempCardParent.childCount;

        for (int i = 0; i < DefaultTempCardParent.childCount; i++)
        {
            if (transform.position.x < DefaultTempCardParent.GetChild(i).position.x)
            {
                newIndex = i;

                if (tempCardGO.transform.GetSiblingIndex() < newIndex)
                {
                    newIndex--;
                }

                break;
            }
        }

        tempCardGO.transform.SetSiblingIndex(newIndex);
    }
}