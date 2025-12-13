using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayingSceneController : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject cardOverlay;

    private void Start()
    {
        CardOverlayManager.Instance.RegisterOverlayGameObject(SceneManager.GetActiveScene().name, cardOverlay);
        RegisterCallbacks();
    }

    private void RegisterCallbacks()
    {
        backButton.onClick.AddListener(() =>
        {
            // var handler = FindAnyObjectByType<NetworkRunnerHandler>();
            // if (handler.Runner != null)
            // {
            //     await handler.Runner.Shutdown();
            //     Destroy(handler.gameObject);
            //     Debug.Log("[UI] Runner shutdown complete");
            // }

            SceneManager.LoadScene("CollectionScene");
        });
    }
}
