using UnityEngine;

public class LoadingSceneController : MonoBehaviour
{
    private bool cardsLoaded = false;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 90;
    }

    private async void Start()
    {
        if (!cardsLoaded)
        {
            Debug.Log("[LoadingScene] Загружаем карты...");
            await CardRepository.Instance.GetAllGameCards();
            cardsLoaded = true;
        }

        InternetChecker.Instance.CheckInternet();
    }
};
