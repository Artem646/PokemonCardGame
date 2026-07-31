using System.Collections.Generic;

public static class AttackAnimationRegistry
{
    private static Dictionary<string, BaseAttackAnimation> animations = new();
    private static BaseAttackAnimation defaultAnimation = new DefaultMeleeAnimation();

    static AttackAnimationRegistry()
    {
        Register("Flash Cannon", new FlashCannonAnimation());
        Register("Aqua Tail", new AquaTailAnimation());
        Register("Rapid Spin", new RapidSpinAnimation());
        Register("Water Gun", new WaterGunAnimation());
        Register("Heat Wave", new HeatWaveAnimation());
        Register("Air Slash", new AirSlashAnimation());
        Register("Slash", new SlashAnimation());
        Register("Vine Whip", new VineWhipAnimation());
        Register("Solar Beam", new SolarBeamAnimation());
        Register("Take Down", new TakeDownAnimation());
        Register("Wave Crash", new WaveCrashAnimation());
        Register("Water Pulse", new WaterPulseAnimation());
        Register("Razor Leaf", new RazorLeafAnimation());
        Register("Tackle", new QuickAttackAnimation());
        Register("Petal Blizzard", new PetalBlizzardAnimation());
        Register("Petal Dance", new PetalDanceAnimation());
        Register("Fire Fang", new FireFangAnimation());
        Register("Scratch", new ScratchAnimation());
        Register("Flare Blitz", new FlareBlitzAnimation());
        Register("Inferno", new InfernoAnimation());
        Register("Poison Sting", new PoisonStingAnimation());
        Register("Electroweb", new ElectrowebAnimation());
        Register("Drill Run", new DrillRunAnimation());
        Register("Bug Strike", new BugStrikeAnimation());
        Register("Ember", new EmberAnimation());
        Register("Bug Bite", new BugBiteAnimation());
        Register("Bug Buzz", new BugBuzzAnimation());
        Register("Quick Attack", new QuickAttackAnimation());
        Register("Wing Attack", new WingAttackAnimation());
        Register("Hurricane", new HurricaneAnimation());
        Register("Brave Bird", new BraveBirdAnimation());
        Register("Gust", new GustAnimation());
        Register("Super Fang", new SuperFangAnimation());
        Register("Dig", new DigAnimation());
        Register("Sludge Bomb", new SludgeBombAnimation());
        Register("Iron Tail", new IronTailAnimation());
        Register("Grass Knot", new GrassKnotAnimation());
        Register("Belch", new BelchAnimation());
        Register("Wrap", new WrapAnimation());
        Register("Spit Up", new SpitUpAnimation());
        Register("Electro Ball", new ElectroBallAnimation());
        Register("Nuzzle", new NuzzleAnimation());
    }

    /* Анимации
1. Ударные
    Register("Rapid Spin", new RapidSpinAnimation());
    Register("Slash", new SlashAnimation());
    Register("Take Down", new QuickAttackAnimation());
    Register("Wave Crash", new WaveCrashAnimation());
    Register("Tackle", new TakeDownAnimation());
    Register("Petal Blizzard", new PetalBlizzardAnimation());
    Register("Fire Fang", new FireFangAnimation());
    Register("Scratch", new ScratchAnimation());
    Register("Flare Blitz", new FlareBlitzAnimation());
    Register("Drill Run", new DrillRunAnimation());
    Register("Bug Strike", new BugStrikeAnimation());
    Register("Bug Bite", new BugBiteAnimation());
    Register("Wing Attack", new WingAttackAnimation());
    Register("Super Fang", new SuperFangAnimation());
    Register("Dig", new DigAnimation());
    Register("Nuzzle", new NuzzleAnimation());

2. Выпускание потока частиц
    Register("Heat Wave", new HeatWaveAnimation());
    Register("Inferno", new InfernoAnimation());
    Register("Ember", new EmberAnimation());
    Register("Gust", new GustAnimation());
    Register("Spit Up", new SpitUpAnimation());
    Register("Electro Ball", new ElectroBallAnimation());

3. Выпускание собственных объектов
    Register("Air Slash", new AirSlashAnimation());
    Register("Vine Whip", new VineWhipAnimation());
    Register("Razor Leaf", new RazorLeafAnimation());
    Register("Electroweb", new ElectrowebAnimation());
    Register("Bug Strike", new BugStrikeAnimation());
    Register("Grass Knot", new GrassKnotAnimation());
    Register("Wrap", new WrapAnimation());

4. Выпускание луча
    Register("Flash Cannon", new FlashCannonAnimation());
    Register("Water Gun", new WaterGunAnimation());
    Register("Solar Beam", new SolarBeamAnimation());
    Register("Water Pulse", new WaterPulseAnimation());

5. С круговым потоком частиц
    Register("Wave Crash", new WaveCrashAnimation());
    Register("Petal Blizzard", new PetalBlizzardAnimation());
    Register("Petal Dance", new PetalDanceAnimation());
    Register("Hurricane", new HurricaneAnimation());

6. Выпускаение хвоста
    Register("Aqua Tail", new AquaTailAnimation());
    Register("Poison Sting", new PoisonStingAnimation());
    Register("Iron Tail", new IronTailAnimation());

7. Появление на карте разрезов и иных объектов
    Register("Slash", new SlashAnimation());
    Register("Scratch", new ScratchAnimation());
    Register("Bug Bite", new BugBiteAnimation());
    Register("Wing Attack", new WingAttackAnimation());
    Register("Hurricane", new HurricaneAnimation());
    Register("Super Fang", new SuperFangAnimation());
    
8. Появление частиц после удара
    Register("Water Gun", new WaterGunAnimation());
    Register("Wave Crash", new WaveCrashAnimation());
    Register("Flare Blitz", new FlareBlitzAnimation());
    Register("Nuzzle", new NuzzleAnimation());
*/

    private static void Register(string abilityName, BaseAttackAnimation animationObject)
    {
        animations[abilityName.ToLower()] = animationObject;
    }

    public static BaseAttackAnimation GetAnimationForAbility(string abilityName)
    {
        if (string.IsNullOrEmpty(abilityName)) return defaultAnimation;

        string key = abilityName.ToLower();

        if (animations.TryGetValue(key, out BaseAttackAnimation anim))
            return anim;

        return defaultAnimation;
    }
}