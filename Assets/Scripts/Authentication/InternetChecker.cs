using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InternetChecker : MonoBehaviour
{
    public static InternetChecker Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LoadingScene")
        {
            VerifyInternetConnection();
        }
    }

    public void VerifyInternetConnection()
    {
        if (!HasInternetAccess())
        {
            ShowNoConnectionDialog(() =>
            {
                FirebaseInitializer.Instance.Initialize();
            });
        }
        else
        {
            FirebaseInitializer.Instance.Initialize();
        }
    }

    public bool HasInternetAccess()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    public void ShowNoConnectionDialog(Action actionIfConnected)
    {
        ConfirmDialogController.ShowDialog(
            () => HandleRetryClicked(actionIfConnected),
            HandleOpenBestiaryClicked,
            HandleExitGameClicked);
    }

    private async void HandleRetryClicked(Action actionIfConnected)
    {
        await Task.Delay(1000);

        if (!HasInternetAccess())
            ShowNoConnectionDialog(actionIfConnected);
        else
        {
            ConfirmDialogController.CloseDialogOverlay();
            actionIfConnected?.Invoke();
        }
    }

    private void HandleOpenBestiaryClicked()
    {
        ConfirmDialogController.CloseDialogOverlay();
        SceneContext.PreviousMenuSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("BestiaryScene");
    }

    private void HandleExitGameClicked()
    {
        Application.Quit();
    }

    public void CheckBeforeAction(Action actionIfConnected)
    {
        if (HasInternetAccess())
            actionIfConnected?.Invoke();
        else
            ShowNoConnectionDialog(actionIfConnected);
    }
}
