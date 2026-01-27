using UnityEngine;

public class SaveData
{
    public bool newGame;
    public int collectedPuzzlesLevel1;
    public int collectedPuzzlesLevel2;
    public int collectedPuzzlesLevel3;

    public Vector2 playerPosition;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    public SaveData data;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        LoadData();
    }

    private void LoadData()
    {
        // Load from PlayerPrefs
        data = new SaveData();
        data.newGame = PlayerPrefs.GetInt("NewGame", 1) == 1;
        data.collectedPuzzlesLevel1 = PlayerPrefs.GetInt("CollectedPuzzlesLevel1", 0);
        data.collectedPuzzlesLevel2 = PlayerPrefs.GetInt("CollectedPuzzlesLevel2", 0);
        data.collectedPuzzlesLevel3 = PlayerPrefs.GetInt("CollectedPuzzlesLevel3", 0);
        data.playerPosition = new Vector2(
            PlayerPrefs.GetFloat("PlayerPosX", -7.29005575f),
            PlayerPrefs.GetFloat("PlayerPosY", -1.14750588f)
        );
    }

    public void SaveData(bool savePosition = false)
    {
        PlayerPrefs.SetInt("NewGame", data.newGame ? 1 : 0);
        PlayerPrefs.SetInt("CollectedPuzzlesLevel1", data.collectedPuzzlesLevel1);
        PlayerPrefs.SetInt("CollectedPuzzlesLevel2", data.collectedPuzzlesLevel2);
        PlayerPrefs.SetInt("CollectedPuzzlesLevel3", data.collectedPuzzlesLevel3);

        if (savePosition)
        {
            Vector2 playerPos = Player.Instance.transform.position;
            data.playerPosition = playerPos;
            PlayerPrefs.SetFloat("PlayerPosX", data.playerPosition.x);
            PlayerPrefs.SetFloat("PlayerPosY", data.playerPosition.y);
        }
        PlayerPrefs.Save();
    }


}
