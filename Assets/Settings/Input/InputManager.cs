using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public PlayerControl Input { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Input = new PlayerControl();

        // Enable only what makes sense at boot
        Input.Player.Enable();
        Input.UI.Disable();
    }

    private void OnDestroy()
    {
        Input?.Dispose();
    }

    // -------- Context Switching --------

    public void EnterGameplay()
    {
        Input.Player.Enable();
        Input.UI.Disable();
    }

    public void EnterUI()
    {
        Input.Player.Disable();
        Input.UI.Enable();
    }

    public void DisableAll()
    {
        Input.Disable();
    }
}
