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
            PokemonElement.Flying, PokemonElement.Electric, PokemonElement.Ground,
            PokemonElement.Fairy, PokemonElement.Normal, PokemonElement.Poison,
            PokemonElement.Steel };

    public FilterPanelView(VisualElement root)
    {
        elementIconsContainer = root.Q<VisualElement>("elementsContainer");
        List<VisualElement> wrappers = elementIconsContainer.Children().ToList();

        for (int i = 0; i < wrappers.Count && i < pokemonElements.Count; i++)
        {
            VisualElement wrapper = wrappers[i];
            PokemonElement type = pokemonElements[i];

            wrapper.userData = type;

            wrapper.RegisterCallback<ClickEvent>(evt =>
            {
                if (wrapper.userData is PokemonElement element)
                    ToggleFilter(element, wrapper);
            });

            VisualElement icon = wrapper.Q<VisualElement>(className: "element-icon");
            SetIconInactive(icon);
        }
    }

    private void ToggleFilter(PokemonElement element, VisualElement wrapper)
    {
        VisualElement icon = wrapper.Q<VisualElement>(className: "element-icon");
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
