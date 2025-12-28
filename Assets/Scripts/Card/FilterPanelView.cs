using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

public class FilterPanelView
{
    private readonly List<PokemonElement> activeFilters = new();
    private readonly VisualElement elementIconsContainer;

    public event Action<List<PokemonElement>, List<PokemonElement>> OnFilterChanged;

    public readonly List<PokemonElement> pokemonElements = new() {
            PokemonElement.Grass, PokemonElement.Fire, PokemonElement.Water,
            PokemonElement.Bug, PokemonElement.Psychic, PokemonElement.Fighting,
            PokemonElement.Flying, PokemonElement.electric, PokemonElement.Ground,
            PokemonElement.Fairy, PokemonElement.Normal, PokemonElement.Poison};

    public FilterPanelView(VisualElement root)
    {
        elementIconsContainer = root.Q<VisualElement>("elementsFilterPanel");
        var icons = elementIconsContainer.Children().ToList();

        for (int i = 0; i < icons.Count && i < pokemonElements.Count; i++)
        {
            VisualElement icon = icons[i];
            PokemonElement type = pokemonElements[i];

            icon.userData = type;

            icon.RegisterCallback<ClickEvent>(evt =>
            {
                if (icon.userData is PokemonElement PokemonElement)
                    ToggleFilter(PokemonElement, icon);
            });

            SetIconInactive(icon);
        }
    }

    private void ToggleFilter(PokemonElement element, VisualElement icon)
    {
        if (activeFilters.Contains(element))
        {
            activeFilters.Remove(element);
            SetIconInactive(icon);
        }
        else
        {
            activeFilters.Add(element);
            SetIconActive(icon);
        }

        OnFilterChanged?.Invoke(new List<PokemonElement>(activeFilters), new List<PokemonElement>(pokemonElements));
    }

    private void SetIconActive(VisualElement icon) =>
        icon.style.unityBackgroundImageTintColor = UnityEngine.Color.white;

    private void SetIconInactive(VisualElement icon) =>
        icon.style.unityBackgroundImageTintColor = new UnityEngine.Color(0.5f, 0.5f, 0.5f);
}
