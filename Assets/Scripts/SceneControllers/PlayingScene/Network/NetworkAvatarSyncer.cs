using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class AvatarStorage
{
    public static Dictionary<int, Texture2D> AvatarCache = new();
    public static byte[] PlayerIdWithImagePacket { get; set; }

    public static void SaveAvatar(int playerId, Texture2D imageTexture) => AvatarCache[playerId] = imageTexture;

    public static Texture2D GetAvatar(int playerId)
    {
        return AvatarCache.TryGetValue(playerId, out var tex) ? tex : null;
    }

    public static void RemoveAvatar(int playerId)
    {
        if (AvatarCache.ContainsKey(playerId))
            AvatarCache.Remove(playerId);
    }

    public static void Clear() => AvatarCache.Clear();
}

public class NetworkAvatarSyncer : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static NetworkAvatarSyncer Instance { get; private set; }
    public event Action<int, Texture2D> OnAvatarReceived;
    private readonly ReliableKey AVATAR_KEY = ReliableKey.FromInts(42, 0, 0, 0);

    public override void Spawned()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            Runner.AddCallbacks(this);
        }
        else
            Runner.Despawn(Object);
    }

    public void PrepareUserImage(Texture2D image)
    {
        if (image == null || Runner == null) return;

        int myId = Runner.LocalPlayer.PlayerId;

        byte[] imageBytes = image.EncodeToJPG(30);
        byte[] idBytes = BitConverter.GetBytes(myId);

        byte[] packet = new byte[idBytes.Length + imageBytes.Length];
        Buffer.BlockCopy(idBytes, 0, packet, 0, idBytes.Length);
        Buffer.BlockCopy(imageBytes, 0, packet, idBytes.Length, imageBytes.Length);

        AvatarStorage.PlayerIdWithImagePacket = packet;
        AvatarStorage.SaveAvatar(myId, image);
        OnAvatarReceived?.Invoke(myId, image);

        Debug.Log($"[AvatarSyncer] Фото подготовлено. Мой ID: {myId}, Размер данных: {AvatarStorage.PlayerIdWithImagePacket.Length} байт");

        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            if (player != Runner.LocalPlayer)
                Runner.SendReliableDataToPlayer(player, AVATAR_KEY, AvatarStorage.PlayerIdWithImagePacket);
        }
    }

    public void RequestAvatars()
    {
        if (Object != null && Object.IsValid)
        {
            Debug.Log("[AvatarSyncer] Широковещательный запрос аватарок...");
            RpcRequestAvatarsFromPlayers(NetworkRunnerController.Instance.NetworkRunner.LocalPlayer);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RpcRequestAvatarsFromPlayers(PlayerRef requester)
    {
        if (AvatarStorage.PlayerIdWithImagePacket != null && requester != Runner.LocalPlayer)
        {
            Debug.Log($"[AvatarSyncer] Отправляю фото игроку {requester.PlayerId}");
            Runner.SendReliableDataToPlayer(requester, AVATAR_KEY, AvatarStorage.PlayerIdWithImagePacket);
        }
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef sender, ReliableKey key, ArraySegment<byte> data)
    {
        if (key == AVATAR_KEY)
        {
            byte[] receivedBytes = data.ToArray();
            if (receivedBytes.Length < 4) return;

            int realSenderId = BitConverter.ToInt32(receivedBytes, 0);

            if (realSenderId == Runner.LocalPlayer.PlayerId)
            {
                Debug.Log("[AvatarSyncer] Проигнорирован пакет с собственным ID внутри.");
                return;
            }

            Debug.Log($"[AvatarSyncer] ПОЛУЧЕН ПАКЕТ. Fusion говорит sender: {sender.PlayerId}, но ВНУТРИ пакета ID: {realSenderId}");

            byte[] imagePart = new byte[receivedBytes.Length - 4];
            Buffer.BlockCopy(receivedBytes, 4, imagePart, 0, imagePart.Length);

            Texture2D receivedTexture = new(2, 2);
            if (receivedTexture.LoadImage(imagePart))
            {
                AvatarStorage.SaveAvatar(realSenderId, receivedTexture);
                OnAvatarReceived?.Invoke(realSenderId, receivedTexture);
            }
        }
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}