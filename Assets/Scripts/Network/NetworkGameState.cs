using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGameState : NetworkBehaviour
{
    [Networked] public NetworkString<_512> FirstPlayerDeckCsv { get; private set; }
    [Networked] public NetworkString<_512> SecondPlayerDeckCsv { get; private set; }

    [Networked] public bool FirstPlayerReady { get; private set; }
    [Networked] public bool SecondPlayerReady { get; private set; }

    private bool initialized = false;
    private GameManagerScript gm;

    public override void Spawned()
    {
        NotificationManager.ShowNotification("Spawned");
        Debug.Log("Spawned");
        Debug.Log($"[NetworkGameState] Spawned | Authority={(Runner.IsServer ? "Server" : "Client")} | Scene={UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

        gm = FindAnyObjectByType<GameManagerScript>();

        if (Runner.IsServer)
        {
            List<int> ids = SelectedDeckManager.GetSelectedDeckIds();
            Debug.Log("[NetworkGameState] Server setting FirstPlayer deck and marking ready");
            SetDeckCsv(true, ids);
        }
        else
        {
            string csv = SelectedDeckManager.GetSelectedDeckCsv();
            Debug.Log("[NetworkGameState] Client sending deck to server via RPC");
            RpcSubmitDeck(csv);
        }
    }

    public override void Render()
    {
        base.Render();

        if (FirstPlayerReady && SecondPlayerReady && !initialized)
        {
            initialized = true;
            if (gm != null)
            {
                gm.Init(this);
                Debug.Log("[NetworkGameState] GameManagerScript.Init вызван через Render");
            }
            else
            {
                Debug.LogWarning("[NetworkGameState] GameManagerScript не найден");
            }
        }
    }

    public void SetDeckCsv(bool isFirstPlayer, List<int> ids)
    {
        string csv = ids == null || ids.Count == 0 ? string.Empty : string.Join(",", ids);

        if (isFirstPlayer)
        {
            FirstPlayerDeckCsv = csv;
            FirstPlayerReady = true;
            Debug.Log($"[NetworkGameState] FirstPlayer deck set: {csv} | FirstPlayerReady={FirstPlayerReady}");
        }
        else
        {
            SecondPlayerDeckCsv = csv;
            SecondPlayerReady = true;
            Debug.Log($"[NetworkGameState] SecondPlayer deck set: {csv} | SecondPlayerReady={SecondPlayerReady}");
        }

        Debug.Log($"Ready flags: FirstPlayerReady={FirstPlayerReady}, SecondPlayerReady={SecondPlayerReady}");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSubmitDeck(string csv)
    {
        Debug.Log($"[NetworkGameState] RpcSubmitDeck received on server | csv={csv}");
        List<int> ids = SelectedDeckManager.ParseCsv(csv);
        SetDeckCsv(false, ids);
    }

    // // Клиент -> Сервер.
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRequestPlayCard(int cardId, int siblingIndex, bool toField, RpcInfo info = default)
    {
        bool isFirstPlayer = info.Source.RawEncoded == 0;
        Debug.Log("RawEncoded" + info.Source.RawEncoded);
        Debug.Log($"Source: {info.Source}, isFirstPlayer={isFirstPlayer}");

        RpcPlayCard(cardId, siblingIndex, isFirstPlayer, toField);
    }

    // Сервер -> Всем
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcPlayCard(int playedCardId, int siblingIndex, bool isFirstPlayer, bool toField)
    {
        FindAnyObjectByType<GameManagerScript>().OnCardPlayed(playedCardId, siblingIndex, isFirstPlayer, toField);
    }

    public List<int> GetPlayerDeckIds() => SelectedDeckManager.ParseCsv(FirstPlayerDeckCsv.Value);
    public List<int> GetEnemyDeckIds() => SelectedDeckManager.ParseCsv(SecondPlayerDeckCsv.Value);
}

