using UnityEngine;

public class LoadingSceneController : MonoBehaviour
{
    private async void Start()
    {
        await CardRepository.Instance.GetAllGameCards();
    }
}
