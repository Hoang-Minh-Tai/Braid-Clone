using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private SceneData_SO sceneToLoad;
    [SerializeField] private int gateIndex;

    public void EnterDoor()
    {
        SceneGameManager.Instance.LoadScene(sceneToLoad, gateIndex);
    }
}
