using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [SerializeField] private GameObject background;
    [SerializeField] private GameObject container;
    [SerializeField] private UI_Page mainMenu;
    [SerializeField] private Menu_EventOption exitCurrentWorldOption;
    [SerializeField] private SceneData_SO introSceneData;
    [SerializeField] private SceneData_SO cloudSceneData;

    [Header("Sounds")]

    private PlayerControl input;

    void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        Instance = this;

        input = InputManager.Instance.Input;
    }

    void Start()
    {
        HideMenu();
    }

    void OnEnable()
    {
        input.Player.Pause.performed += ctx => ShowMenu();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        input.Player.Pause.performed -= ctx => ShowMenu();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Intro")
        {
            exitCurrentWorldOption.SetDisabled(true);
        }
        else
        {
            exitCurrentWorldOption.SetDisabled(false);
        }
    }



    public void ShowMenu()
    {
        GameEventManager.instance.generalEvent.OpenMainMenu();

        Time.timeScale = 0;
        InputManager.Instance.EnterUI();

        background.SetActive(true);
        container.SetActive(true);
        mainMenu.PageEnter();
        mainMenu.PlayPageTransitionSound();
    }

    public void HideMenu()
    {
        GameEventManager.instance.generalEvent.CloseMainMenu();

        InputManager.Instance.EnterGameplay();
        Time.timeScale = 1;

        background.SetActive(false);
        container.SetActive(false);
    }

    public void ExitCurrentWorld()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "Intro")
        {
            return;
        }
        else if (currentSceneName == "Cloud")
        {
            SceneGameManager.Instance.LoadScene(introSceneData, 0);
        }
        else if (currentSceneName == "Stage 1 Level 1")
        {
            SceneGameManager.Instance.LoadScene(cloudSceneData, 1);
        }
        else if (currentSceneName == "Stage 1 Level 2")
        {
            SceneGameManager.Instance.LoadScene(cloudSceneData, 2);
        }
        else if (currentSceneName == "Stage 1 Level 3")
        {
            SceneGameManager.Instance.LoadScene(cloudSceneData, 3);
        }

        HideMenu();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
