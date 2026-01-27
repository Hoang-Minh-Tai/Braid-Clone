using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager instance;

    public GeneralEvent generalEvent;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            generalEvent = new GeneralEvent();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
    }
}
