using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class SceneGameManager : MonoBehaviour
{
    public static SceneGameManager Instance;

    private SceneData_SO pendingSceneData;
    private int pendingGateIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(SceneData_SO sceneData, int gateIndex)
    {
        Debug.Log($"SceneGameManager: Loading scene {sceneData.sceneName} via gate {gateIndex}");
        pendingSceneData = sceneData;
        pendingGateIndex = gateIndex;

        if (SceneManager.GetActiveScene().name == "Intro") SaveManager.Instance.SaveData(savePosition: true);
        else SaveManager.Instance.SaveData();
        SceneManager.LoadScene(sceneData.sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingSceneData == null)
            return;

        Player player = Player.Instance;
        if (player == null)
            return;

        Vector3 oldPos = player.transform.position;
        Vector3 newPos = pendingSceneData.gates[pendingGateIndex].location;

        // Teleport player
        player.transform.position = newPos;
        float scaleX = pendingSceneData.gates[pendingGateIndex].direction;
        player.movement.FlipForce(scaleX);


        // Warp Cinemachine camera
        WarpCinemachine(player.transform);
        RewindableMusic.PlayMusic(pendingSceneData.backgroundMusic, pendingSceneData.backgroundMusic_Reverse);

        pendingSceneData = null;
    }

    private void WarpCinemachine(Transform player)
    {
        var vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam == null)
            return;
        vcam.enabled = false;
        Camera.main.transform.position = player.position;
        vcam.enabled = true;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
