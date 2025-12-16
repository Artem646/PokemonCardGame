using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InternetChecker : MonoBehaviour
{
    public static InternetChecker Instance { get; private set; }
    private bool dialogVisible;

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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LoadingScene")
        {
            Debug.Log("[InternetChecker] Загрузилась сцена LoadingScene — проверяем интернет...");
            CheckInternet();
        }
    }

    public void CheckInternet()
    {
        if (!IsConnected())
        {
            Debug.Log("[InternetChecker] Интернет не найден после загрузки данных, показываем окно...");
            ShowNoConnectionDialog(() =>
            {
                Debug.Log("[InternetChecker] Интернет восстановлен, продолжаем работу...");
                FirebaseInitializer.Instance.Initialize();
            });
        }
        else
        {
            FirebaseInitializer.Instance.Initialize();
        }
    }

    public bool IsConnected()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    public void ShowNoConnectionDialog(Action onRetry = null)
    {
        if (dialogVisible) return;
        dialogVisible = true;

        ConfirmDialogController.ShowDialog(
            "Нет подключения к интернету.",
            onRetry: async () =>
            {
                ConfirmDialogController.CloseDialog();
                dialogVisible = false;
                await Task.Delay(1000);

                if (!IsConnected())
                {
                    Debug.Log("[InternetChecker] Интернет всё ещё отсутствует, показываем окно снова...");
                    ShowNoConnectionDialog(onRetry);
                }
                else
                {
                    onRetry?.Invoke();
                }
            },
            onBestiaty: () =>
            {
                dialogVisible = false;
                ConfirmDialogController.CloseDialog();
                SceneContext.PreviousSceneName = SceneManager.GetActiveScene().name;
                SceneManager.LoadScene("BestiaryScene");
            },
            onCancel: () =>
            {
                dialogVisible = false;
                ConfirmDialogController.CloseDialog();
                Application.Quit();
            }
        );
    }

    public void CheckBeforeAction(Action onConnected)
    {
        if (IsConnected())
        {
            onConnected?.Invoke();
        }
        else
        {
            ShowNoConnectionDialog(onConnected);
        }
    }
}
