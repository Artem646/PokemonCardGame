using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class DeckSelectionDialogController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset deckCardTemplate;

    private VisualElement root;
    private VisualElement overlay;
    private DropdownField deckDropdown;
    private VisualElement deckCardsContainer;
    private VisualElement cardRoot;
    private Label deckNotSelectedLabel;
    private Button closeDeckDialogButton;

    private void Start()
    {
        InitializeUI();
        RefreshDeckDropdown();
        RegisterCallbacks();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;
        overlay = root.Q<VisualElement>("overlay");
        deckDropdown = root.Q<DropdownField>("deckSelectField");
        deckCardsContainer = root.Q<VisualElement>("deckCardsContainer");
        deckNotSelectedLabel = root.Q<Label>("deckNotSelectedLabel");
        closeDeckDialogButton = root.Q<Button>("closeDeckDialogButton");
    }

    public void RefreshDeckDropdown()
    {
        User user = UserSession.Instance.ActiveUser;
        if (user != null && user.decks.Count > 0)
            deckDropdown.choices = user.decks.Select(deck => deck.name).ToList();
        else
        {
            LocalizedString localizedValue = new("ElementsText", "EmptyDropdown");
            localizedValue.StringChanged += (str) =>
            {
                deckDropdown.choices = new() { str };
                deckDropdown.value = str;
            };
        }
    }

    public void OpenDeckSelectionDialog()
    {
        if (SelectedDeckManager.SelectedDeck != null)
        {
            deckNotSelectedLabel.style.display = DisplayStyle.None;
            AddDeckCardsToContainer(SelectedDeckManager.SelectedDeck);
        }
        else
            deckNotSelectedLabel.style.display = DisplayStyle.Flex;

        overlay.style.display = DisplayStyle.Flex;
    }

    private void AddDeckCardsToContainer(Deck deck)
    {
        deckCardsContainer.Clear();

        List<CardModel> result = new();
        foreach (int id in deck.cards)
        {
            CardModel cardModel = CardRepository.Instance.GetGameCardModelById(id);
            if (cardModel != null) result.Add(cardModel);
        }

        foreach (CardModel cardModel in result)
        {
            cardRoot = deckCardTemplate.Instantiate();

            cardRoot.Q<VisualElement>("fullCard").style.backgroundColor = new StyleColor(cardModel.colors.cardColor);
            Label title = cardRoot.Q<Label>("title");

            LocalizedString localizedText = new("PokemonTitles", cardModel.titleKey);
            localizedText.StringChanged += (str) =>
            {
                title.text = @$"<color=white><gradient=""TextGradient"">{str}</gradient></color>";

                string currentLang = LocalizationSettings.SelectedLocale.Identifier.Code;
                if (currentLang.StartsWith("ru") || currentLang.StartsWith("be"))
                {
                    title.style.fontSize = 20f;
                    title.style.letterSpacing = cardModel.uiToolkitVisualData.letterSpacingRU;
                    title.style.unityFontStyleAndWeight = FontStyle.Bold;
                }
                else
                {
                    title.style.fontSize = 27f;
                    title.style.letterSpacing = cardModel.uiToolkitVisualData.letterSpacingEN;
                    title.style.unityFontStyleAndWeight = FontStyle.Normal;
                }
            };

            CardViewHelper.UpdateBodyUIToolkit(cardRoot, cardModel);
            CardViewHelper.SetBodyImagesUIToolkit(cardRoot, cardModel);
            cardRoot.Q<Label>("cardNumberLabel").text = cardModel.id.ToString();

            cardRoot.AddToClassList("small-deck-card");
            deckCardsContainer.Add(cardRoot);
        }
    }

    private void RegisterCallbacks()
    {
        deckDropdown.RegisterValueChangedCallback(evt =>
        {
            User user = UserSession.Instance.ActiveUser;
            Deck selectedDeck = user.decks.FirstOrDefault(deck => deck.name == evt.newValue);
            if (selectedDeck != null)
            {
                deckNotSelectedLabel.style.display = DisplayStyle.None;
                AddDeckCardsToContainer(selectedDeck);
                SelectedDeckManager.SetSelectedDeck(selectedDeck);
                NotificationManager.ShowNotification($@"Колода ""{selectedDeck.name}"" выбрана", NotificationType.Success);
            }
        });

        closeDeckDialogButton.RegisterCallback<ClickEvent>(evt =>
        {
            overlay.style.display = DisplayStyle.None;
        });
    }
}
