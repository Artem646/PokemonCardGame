using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CollectionSceneController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset cardTemplate;

    private VisualElement userImage;
    private Label userName;
    private List<string> filterElements = new List<string>();
    private List<CardView> cardViews = new List<CardView>();

    private void Start()
    {
        ScrollView cardsContainer = uiDocument.rootVisualElement.Q<ScrollView>("cardScrollView");
        VisualElement overlay = uiDocument.rootVisualElement.Q<VisualElement>("overlay");
        CardCollectionPresenter cardPresenter = new CardCollectionPresenter(cardsContainer, cardTemplate, overlay);

        List<CardData> cards = CardRepository.Instance.GetCachedCards();
        cardPresenter.ReloadCards(cards);

        cardViews = cardPresenter.GetAllCardViews();

        userImage = uiDocument.rootVisualElement.Q<VisualElement>("userImage");
        userName = uiDocument.rootVisualElement.Q<Label>("userName");

        string photoUrl = FirebaseService.Instance.GetAuth().CurrentUser.PhotoUrl?.ToString();
        StartCoroutine(LoadUserImage(photoUrl));

        userName.text = FirebaseService.Instance.GetAuth().CurrentUser.DisplayName;

        VisualElement elementIconsContainer = uiDocument.rootVisualElement.Q<VisualElement>("elementsIconsContainer");

        foreach (VisualElement icon in elementIconsContainer.Children())
        {
            icon.RegisterCallback<ClickEvent>(evt =>
            {
                ToggleElementFilter(icon.name, icon);
                ApplyElementFilter();
            });

            SetIconInactive(icon);
        }

        uiDocument.rootVisualElement.Q<Button>("playButton").RegisterCallback<ClickEvent>(evt =>
        {
            SceneManager.LoadScene("PlayScene");
        });
    }

    private IEnumerator LoadUserImage(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[P][ImageLoader] Ошибка загрузки изображения: " + request.error);
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);
        userImage.style.backgroundImage = new StyleBackground(texture);

        Debug.Log("[P][ImageLoader] Изображение пользователя установлено.");
    }

    private void ToggleElementFilter(string element, VisualElement icon)
    {
        if (filterElements.Contains(element))
        {
            filterElements.Remove(element);
            SetIconInactive(icon);
        }
        else
        {
            filterElements.Add(element);
            SetIconActive(icon);
        }
    }

    private void SetIconActive(VisualElement icon)
    {
        icon.style.unityBackgroundImageTintColor = Color.white;
    }

    private void SetIconInactive(VisualElement icon)
    {
        icon.style.unityBackgroundImageTintColor = new Color(0.5f, 0.5f, 0.5f);
    }

    private void ApplyElementFilter()
    {
        // foreach (var cardVisualElement in cardsVisualElements)
        // {
        //     if (filterElements.Count == 0 || IsFilterElement(cardVisualElement))
        //     {
        //         cardVisualElement.style.opacity = 1f;
        //     }
        //     else
        //     {
        //         cardVisualElement.style.opacity = 0.3f;
        //     }
        // }
    }

    private bool IsFilterElement(VisualElement cardVisualElement)
    {
        string mainElement = cardVisualElement.Q<VisualElement>("mainElement").userData as string;
        string secondaryElement = cardVisualElement.Q<VisualElement>("secondaryElement").userData as string;

        // if (filterElements.Count == 1)
        // {
        //     string filterElement = filterElements[0];
        //     return mainElement == filterElement || secondaryElement == filterElement;
        // }

        // if (filterElements.Count == 2)
        // {
        //     return (mainElement == filterElements[0] && secondaryElement == filterElements[1]) ||
        //            (mainElement == filterElements[1] && secondaryElement == filterElements[0]);
        // }

        // return false;

        return filterElements.Contains(mainElement) || filterElements.Contains(secondaryElement);
    }
}