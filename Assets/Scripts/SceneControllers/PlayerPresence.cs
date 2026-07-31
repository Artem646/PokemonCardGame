using System;
using Fusion;
using UnityEngine;

public class PlayerPresence : NetworkBehaviour
{
    [Networked] public bool IsSpectator { get; set; }
    [Networked] public PlayerRef PlayerRef { get; set; }

    public static event Action OnPresenceChanged;

    public override void Spawned()
    {
        DontDestroyOnLoad(gameObject);

        if (HasStateAuthority)
        {
            IsSpectator = ConnectionConfig.IsSpectator;
            PlayerRef = Runner.LocalPlayer;
            Debug.Log($"[PlayerPresence] Я заспавнил визитку. Зритель: {IsSpectator}");
        }

        OnPresenceChanged?.Invoke();
    }

    public override void Despawned(NetworkRunner runner, bool hasState) => OnPresenceChanged?.Invoke();
}