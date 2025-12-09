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
            SceneManager.LoadScene("CollectionScene");
        });
    }
}
