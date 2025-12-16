using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "TypeChart", menuName = "Game/TypeChart")]
public class TypeChart : ScriptableObject
{
    [System.Serializable]
    public struct TypePair
    {
        public PokemonElement attacker;
        public PokemonElement defender;
        public float multiplier;
    }

    public List<TypePair> pairs;

    public float GetMultiplier(PokemonElement attacker, PokemonElement defender)
    {
        TypePair pair = pairs.FirstOrDefault(p => p.attacker == attacker && p.defender == defender);
        return pair.multiplier == 0 ? 1f : pair.multiplier;
    }
}
