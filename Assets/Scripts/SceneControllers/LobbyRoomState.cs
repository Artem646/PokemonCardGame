using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LobbyRoomState : NetworkBehaviour
{
    [Networked] public NetworkString<_32> HostName { get; set; }
    [Networked] public NetworkString<_32> ClientName { get; set; }

    public static event Action OnNamesChanged;

    private ChangeDetector changeDetector;
    private LobbySceneController lobbySceneController;

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (!ConnectionConfig.IsSpectator)
        {
            string myRealName = UserSession.Instance.ActiveUser.userData.userName;
            if (Object.HasStateAuthority) HostName = myRealName;
            else RpcSetClientName(myRealName);
        }

        OnNamesChanged?.Invoke();
    }

    public override void Render()
    {
        foreach (string change in changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(HostName):
                case nameof(ClientName):
                    OnNamesChanged?.Invoke();
                    break;
            }
        }
    }

    public PlayerPresence FindPresenceForClient()
    {
        PlayerPresence[] allPresences = FindObjectsByType<PlayerPresence>(FindObjectsInactive.Exclude);

        foreach (PlayerPresence presence in allPresences)
        {
            if (!presence.IsSpectator && presence.Object.InputAuthority != Object.StateAuthority)
                return presence;
        }
        return null;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSetClientName(NetworkString<_32> name, RpcInfo info = default)
    {
        PlayerPresence[] allPresences = FindObjectsByType<PlayerPresence>(FindObjectsInactive.Exclude);
        List<PlayerPresence> realPlayers = allPresences.Where(p => !p.IsSpectator).OrderBy(p => p.Object.InputAuthority.PlayerId).ToList();
        if (realPlayers.Count >= 2 && info.Source != realPlayers[1].Object.InputAuthority)
        {
            Debug.LogWarning($"[Lobby] Отклонена попытка установки имени от лишнего игрока {info.Source}");
            return;
        }

        ClientName = name;
    }

    public void ClearClientName()
    {
        if (Object.HasStateAuthority)
            ClientName = default;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcKickPlayer(int targetPlayerId)
    {
        if (Runner.LocalPlayer.PlayerId == targetPlayerId)
        {
            NotificationManager.ShowNotification("Вы были исключены из комнаты!", NotificationType.Error);
            lobbySceneController = FindAnyObjectByType<LobbySceneController>();
            lobbySceneController.HandleKicked("Вы были исключены из комнаты!");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcKickForLimit(int targetPlayerId)
    {
        if (Runner.LocalPlayer.PlayerId == targetPlayerId)
        {
            NotificationManager.ShowNotification("Места для игроков уже заняты. Можно зайти посмотреть игру!", NotificationType.Error);
            lobbySceneController = FindAnyObjectByType<LobbySceneController>();
            lobbySceneController.HandleKicked("Места для игроков уже заняты!");
        }
    }
}