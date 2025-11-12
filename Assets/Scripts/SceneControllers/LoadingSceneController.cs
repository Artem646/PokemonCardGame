using UnityEngine;

public class LoadingSceneController : MonoBehaviour
{
    private bool cardsLoaded = false;

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
