using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController instance;

    [SerializeField] GameObject mainMenu, loginUi, registerUi;
    [SerializeField] TextMeshProUGUI userNameText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void LoginScreen()
    {
        registerUi.SetActive(false);
        mainMenu.SetActive(false);
        loginUi.SetActive(true);
    }
    public void RegisterScreen()
    {
        loginUi.SetActive(false);
        registerUi.SetActive(true);
    }
    public void MainMenu()
    {
        loginUi.SetActive(false);
        registerUi.SetActive(false);
        mainMenu.SetActive(true);
        userNameText.text = $"User: {FirebaseManager.Instance.User.DisplayName}";
        FirebaseManager.Instance.LoadBoard();
    }
    public void StartGame() => SceneManager.LoadScene("Game scene");
    public void QuitGame() => Application.Quit();
}
